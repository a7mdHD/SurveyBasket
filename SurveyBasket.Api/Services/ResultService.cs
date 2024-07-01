using SurveyBasket.Api.Contracts.Questions;
using SurveyBasket.Api.Contracts.Results;

namespace SurveyBasket.Api.Services;

public class ResultService(ApplicationDbContext context) : IResultService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
    {
       var pollVotes = await _context.Polls
            .Where(x => x.Id == pollId)
            .Select(x => new PollVotesResponse(
                x.Title,
                x.Votes.Select(v => new VoteResponse(
                    $"{ v.User.FirstName} {v.User.LastName}",
                        v.SubmittedOn,
                        v.VoteAnswers.Select(a => new QuestionAnswerRersponse( a.Question.Content, a.Answer.Content)))
            )))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        return pollVotes is null
            ? Result.Failure<PollVotesResponse>(PollError.PollNotFound)
            : Result.Success(pollVotes);
    }

    public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var ispollFound = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);

        if (!ispollFound)
            return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollError.PollNotFound);

        var votesPerDay = await _context.Votes
            .Where(x => x.PollId == pollId)
            .GroupBy(x => new { Date = DateOnly.FromDateTime(x.SubmittedOn) })
            .Select(g => new VotesPerDayResponse(
                    g.Key.Date,
                    g.Count()
                ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);
    }

    public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var ispollFound = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);

        if (!ispollFound)
            return Result.Failure<IEnumerable<VotesPerQuestionResponse>>(PollError.PollNotFound);

        var votesPerQuestion = await _context.VoteAnswers
            .Where(x => x.Vote.PollId == pollId)
            .Select(x => new VotesPerQuestionResponse(
                    x.Question.Content,
                    x.Question.Votes
                        .GroupBy(x => new { AnswerId = x.AnswerId, AnswerContent = x.Answer.Content })
                        .Select(g => new VotesPerAnswerResponse(
                            g.Key.AnswerContent,
                            g.Count()
                            ))
            ))
            .ToListAsync(cancellationToken);
            
        return Result.Success<IEnumerable<VotesPerQuestionResponse>>(votesPerQuestion);
    }
}
