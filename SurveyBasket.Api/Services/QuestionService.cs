using SurveyBasket.Api.Contracts.Answers;
using SurveyBasket.Api.Contracts.Common;
using SurveyBasket.Api.Contracts.Questions;
using System.Linq.Dynamic.Core;

namespace SurveyBasket.Api.Services;

public class QuestionService(ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<QuestionService> logger) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger _logger = logger;

    //private readonly ILogger _logger = logger;
    private const string cachePrefix = "availableQuestions";

    public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .Where(x => x.PollId == pollId && x.Id == id)
            .Include(q => q.Answers)
            .ProjectToType<QuestionResponse>()
            .SingleOrDefaultAsync(cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionError.QuestionNotFound);

        return Result.Success(question);
    }

    public async Task<Result<PaginatedList<QuestionResponse>>> GetAllAsync(int pollId,RequestFilters filters, CancellationToken cancellationToken)
    {
        var ispollFound = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);

        if (!ispollFound)
            return Result.Failure<PaginatedList<QuestionResponse>>(PollError.PollNotFound);

        var query = _context.Questions
            .Where(x => x.PollId == pollId && (string.IsNullOrEmpty(filters.SearchValue) || x.Content.Contains(filters.SearchValue)));

        if(!string.IsNullOrEmpty(filters.SortColumn))
        {
            query = query.OrderBy($"{filters.SortColumn} {filters.SortDirection}");
        }
        
        var source = query
                        .Include(x => x.Answers)
                        //.Select(q => new QuestionResponse(
                        //    q.PollId,
                        //    q.Content,
                        //    q.Answers.Select(a => new AnswerResponse(a.Id, a.Content))))
                        .ProjectToType<QuestionResponse>()
                        .AsNoTracking();

        var questions = await PaginatedList<QuestionResponse>.CreateAsync(source, filters.PageNumber, filters.PageSize, cancellationToken);

        return Result.Success(questions);
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId, CancellationToken cancellationToken = default)
    {
        var hasVote = await _context.Votes
            .AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);

        if(hasVote)
            return Result.Failure<IEnumerable<QuestionResponse>>(VoteError.DuplicatedVote);

        var pollIsExists = await _context.Polls
            .AnyAsync(x => x.Id == pollId && x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);


        if(!pollIsExists)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollError.PollNotFound);

        var cacheKey = $"{_cacheService}-{pollId}";

        var cachedValue = await _cacheService.GetAsync<IEnumerable<QuestionResponse>>(cacheKey, cancellationToken);

        IEnumerable<QuestionResponse> questions = [];

        if(cachedValue is null)
        {
            _logger.LogInformation("Select questions form database");
            questions = await _context.Questions
            .Where(x => x.PollId == pollId && x.IsActive)
            .Include(x => x.Answers)
            .Select(q => new QuestionResponse(
                q.Id,
                q.Content,
                q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

            await _cacheService.SetAsync(cacheKey, questions, cancellationToken);
        }
        else
        {
            _logger.LogInformation("Select questions form cache");
            questions = cachedValue;
        }
         
        return Result.Success<IEnumerable<QuestionResponse>>(questions);
    }


    public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var ispollFound = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);

        if (!ispollFound)
            return Result.Failure<QuestionResponse>(PollError.PollNotFound);

        var isQuestionFound = await _context.Questions.AnyAsync(q => q.Content == request.Content && q.PollId == pollId, cancellationToken: cancellationToken);

        if (isQuestionFound)
            return Result.Failure<QuestionResponse>(QuestionError.DuplicatedQuestionContent);

        var question = request.Adapt<Question>();
        question.PollId = pollId;

        // Made in MappingConfiguration instead
        //request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));


        await _context.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"{_cacheService}-{pollId}", cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());
    }

    public async Task<Result> UpdateAsync(int pollId,int id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var questionIsExists = await _context.Questions
            .AnyAsync(x => x.PollId == pollId && x.Content == request.Content && x.Id != id,cancellationToken);

        if (questionIsExists)
            return Result.Failure(QuestionError.DuplicatedQuestionContent);

        var question = await _context.Questions
           .Include(q => q.Answers)
           .SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id,cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionError.QuestionNotFound);

        question.Content = request.Content;

        //current answers in db
        var currentAnswers = question.Answers.Select(x => x.Content).ToList();

        // new answers
        var newAnswers = request.Answers.Except(currentAnswers).ToList();

        //add new answers
        newAnswers.ForEach(answer =>
            question.Answers.Add(new Answer { Content = answer }
        ));

        question.Answers.ToList().ForEach(answer =>
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"{_cacheService}-{pollId}", cancellationToken);

        return Result.Success();
    }


    public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
             .SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionError.QuestionNotFound);

        question.IsActive = !question.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"{_cacheService}-{pollId}", cancellationToken);

        return Result.Success();
    }
}
