using SurveyBasket.Api.Contracts.Answers;
using SurveyBasket.Api.Contracts.Questions;
using System.Runtime.InteropServices;
using System.Threading;

namespace SurveyBasket.Api.Services;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;

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

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken)
    {
        var ispollFound = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);

        if (!ispollFound)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollError.PollNotFound);

        var questions = await _context.Questions
            .Where(x => x.PollId == pollId)
            .Include(x => x.Answers)
            //.Select(q => new QuestionResponse(
            //    q.PollId,
            //    q.Content,
            //    q.Answers.Select(a => new AnswerResponse(a.Id, a.Content))))
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .ToListAsync();

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

        return Result.Success();
    }
}
