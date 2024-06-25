using Microsoft.EntityFrameworkCore;
using SurveyBasket.Api.Contracts.Questions;
using SurveyBasket.Api.Contracts.Votes;
using System.Linq;

namespace SurveyBasket.Api.Services;

public class VoteService(ApplicationDbContext context) : IVoteService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default)
    {
        var hasVote = await _context.Votes
            .AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);

        if (hasVote)
            return Result.Failure<IEnumerable<QuestionResponse>>(VoteError.DuplicatedVote);

        var pollIsExists = await _context.Polls
            .AnyAsync(x => x.Id == pollId && x.IsPublished &&
                x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) &&
                x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        if (!pollIsExists)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollError.PollNotFound);

        var availabelQuestions = await _context.Questions
            .Where(x => x.PollId == pollId && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (!request.Answers.Select(x => x.QuestionId).SequenceEqual(availabelQuestions))
            return Result.Failure(VoteError.InvalidQuestions);

        var availabelAnswersWithQuestion = await _context.Answers
            .Where(x => availabelQuestions.Contains(x.QuestionId) && x.IsActive)
            .Select(x => new { AnswerId = x.Id, x.QuestionId })
            .ToListAsync(cancellationToken);

        var requestAnswersWithQuestions = request.Answers.Select(x => new { x.AnswerId, x.QuestionId }).ToList();

        if(requestAnswersWithQuestions.Except(availabelAnswersWithQuestion).Any())
            return Result.Failure(VoteError.InvalidQuestions);


        var vote = new Vote
        {
            UserId = userId,
            PollId = pollId,
            VoteAnswers = request.Answers.Adapt<IEnumerable<VoteAnswer>>().ToList()
        };

        await _context.Votes.AddAsync(vote, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
