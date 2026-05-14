using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Heritage_of_Turkey.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Contact";
            ViewBag.PageDescription = "Send us your questions, feedback, or suggestions about Turkey's museums and ruins.";

            return View(BuildContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactViewModel model)
        {
            ViewData["Title"] = "Contact";
            ViewBag.PageDescription = "Send us your questions, feedback, or suggestions about Turkey's museums and ruins.";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = GetCurrentUserEmail();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userEmail))
            {
                return Challenge();
            }

            ModelState.Remove(nameof(model.Name));
            ModelState.Remove(nameof(model.Email));

            if (!ModelState.IsValid)
            {
                model.Name = GetCurrentUserDisplayName();
                model.Email = userEmail;
                return View(model);
            }

            var now = DateTime.Now;
            var contactMessage = new ContactMessage
            {
                UserId = userId,
                Name = GetCurrentUserDisplayName(),
                Email = userEmail,
                Subject = string.IsNullOrWhiteSpace(model.Subject) ? null : model.Subject.Trim(),
                Message = model.Message.Trim(),
                IsRead = false,
                Status = ContactMessageStatus.Unread,
                SentDate = now,
                CreatedAt = now
            };

            await _context.ContactMessages.AddAsync(contactMessage);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your message has been sent successfully. Thank you for contacting us.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Replies()
        {
            ViewData["Title"] = "My Contact Replies";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var messages = await _context.ContactMessages
                .AsNoTracking()
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new ContactReplyItemViewModel
                {
                    ContactMessageId = m.ContactMessageId,
                    Subject = m.Subject,
                    Message = m.Message,
                    AdminReply = m.AdminReply,
                    Status = m.Status,
                    CreatedAt = m.CreatedAt,
                    RepliedAt = m.RepliedAt
                })
                .ToListAsync();

            var viewModel = new ContactRepliesViewModel
            {
                UserEmail = GetCurrentUserEmail(),
                Messages = messages
            };

            return View(viewModel);
        }

        private ContactViewModel BuildContactViewModel()
        {
            return new ContactViewModel
            {
                Name = GetCurrentUserDisplayName(),
                Email = GetCurrentUserEmail()
            };
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? string.Empty;
        }

        private string GetCurrentUserDisplayName()
        {
            var email = GetCurrentUserEmail();
            return User.Identity?.Name ?? email;
        }
    }
}
