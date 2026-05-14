using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Heritage_of_Turkey.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/ContactMessages")]
    public class ContactMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("", Name = "AdminContactMessages")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new ContactMessageItemViewModel
                {
                    ContactMessageId = m.ContactMessageId,
                    Name = m.Name,
                    Email = m.Email,
                    Subject = m.Subject,
                    Message = m.Message,
                    IsRead = m.IsRead,
                    Status = m.Status,
                    SentDate = m.SentDate,
                    ReadDate = m.ReadDate,
                    CreatedAt = m.CreatedAt,
                    RepliedAt = m.RepliedAt
                })
                .ToListAsync();

            var viewModel = new ContactMessageListViewModel
            {
                Messages = messages,
                TotalCount = messages.Count,
                UnreadCount = messages.Count(m => m.Status == ContactMessageStatus.Unread),
                RepliedCount = messages.Count(m => m.Status == ContactMessageStatus.Replied)
            };

            ViewData["Title"] = "Contact Messages";
            ViewBag.UnreadCount = viewModel.UnreadCount;

            return View("~/Views/Admin/ContactMessages/Index.cshtml", viewModel);
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var message = await _context.ContactMessages.FirstOrDefaultAsync(m => m.ContactMessageId == id);

            if (message == null)
            {
                TempData["ErrorMessage"] = "Contact message could not be found.";
                return RedirectToRoute("AdminContactMessages");
            }

            if (message.Status == ContactMessageStatus.Unread)
            {
                message.Status = ContactMessageStatus.Read;
                message.IsRead = true;
                message.ReadDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            ViewData["Title"] = "Contact Message Details";

            return View("~/Views/Admin/ContactMessages/Details.cshtml", ToDetailViewModel(message));
        }

        [HttpPost("Reply/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, ContactMessageDetailViewModel model)
        {
            if (id != model.ContactMessageId)
            {
                TempData["ErrorMessage"] = "Invalid contact message request.";
                return RedirectToRoute("AdminContactMessages");
            }

            var message = await _context.ContactMessages.FirstOrDefaultAsync(m => m.ContactMessageId == id);

            if (message == null)
            {
                TempData["ErrorMessage"] = "Contact message could not be found.";
                return RedirectToRoute("AdminContactMessages");
            }

            if (string.IsNullOrWhiteSpace(model.ReplyText))
            {
                ModelState.AddModelError(nameof(model.ReplyText), "Reply text is required.");
            }

            if (!ModelState.IsValid)
            {
                var viewModel = ToDetailViewModel(message);
                viewModel.ReplyText = model.ReplyText;
                ViewData["Title"] = "Contact Message Details";
                return View("~/Views/Admin/ContactMessages/Details.cshtml", viewModel);
            }

            var now = DateTime.Now;
            message.AdminReply = model.ReplyText.Trim();
            message.Status = ContactMessageStatus.Replied;
            message.IsRead = true;
            message.ReadDate ??= now;
            message.RepliedAt = now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reply saved successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private static ContactMessageDetailViewModel ToDetailViewModel(ContactMessage message)
        {
            return new ContactMessageDetailViewModel
            {
                ContactMessageId = message.ContactMessageId,
                Name = message.Name,
                Email = message.Email,
                Subject = message.Subject,
                Message = message.Message,
                AdminReply = message.AdminReply,
                Status = message.Status,
                CreatedAt = message.CreatedAt,
                ReadDate = message.ReadDate,
                RepliedAt = message.RepliedAt,
                ReplyText = message.AdminReply ?? string.Empty
            };
        }
    }
}
