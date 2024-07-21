
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using SurveyBasket.Api.Helpers;

namespace SurveyBasket.Api.Services;

public class NotificationService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender) : INotificationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task SendNewPollsNotification(int? pollId = null)
    {
        IEnumerable<Poll> polls = [];

        if(pollId.HasValue)
        {
            var poll = await _context.Polls.SingleOrDefaultAsync(x => x.Id == pollId && x.IsPublished);

            polls = [poll!];
        }
        else
        {
            polls = await _context.Polls
                .Where(x => x.IsPublished && x.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
                .AsNoTracking()
                .ToListAsync();
        }

        //TODO Select members only

        var users = await _userManager.Users.ToListAsync();

        var origins = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        foreach(var poll in polls)
        {
            foreach(var user in users)
            {
                var placeHolders = new Dictionary<string, string>
                {
                    { "{{name}}", user.FirstName },
                    { "{{pollTill}}", poll.Title },
                    { "{{endDate}}", poll.EndsAt.ToString() },
                    { "{{url}}", $"{ origins }/polls/start/{ poll.Id }" }
                };

                var body = EmailBodyBuilder.GenerateEmailBody("PollNotification", placeHolders);

                await _emailSender.SendEmailAsync(user.Email!, $"🎉 Basket Survy: New Poll - { poll.Title }", body);
            }
        }

    }
}
