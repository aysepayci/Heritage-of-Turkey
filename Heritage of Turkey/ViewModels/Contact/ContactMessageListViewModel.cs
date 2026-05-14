using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using Heritage_of_Turkey.Models;

namespace Heritage_of_Turkey.ViewModels.Contact
{
    public class ContactMessageListViewModel
    {
        public IList<ContactMessageItemViewModel> Messages { get; set; } = new List<ContactMessageItemViewModel>();

        public int TotalCount { get; set; }

        public int UnreadCount { get; set; }

        public int RepliedCount { get; set; }
    }

    public class ContactMessageItemViewModel
    {
        public int ContactMessageId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Subject { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public ContactMessageStatus Status { get; set; }

        public DateTime SentDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RepliedAt { get; set; }
    }

    public class ContactMessageDetailViewModel
    {
        public int ContactMessageId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Subject { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? AdminReply { get; set; }

        public ContactMessageStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReadDate { get; set; }

        public DateTime? RepliedAt { get; set; }

        [Required(ErrorMessage = "Reply text is required")]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Reply must be between 3 and 1000 characters")]
        [Display(Name = "Admin Reply")]
        public string ReplyText { get; set; } = string.Empty;
    }

    public class ContactRepliesViewModel
    {
        public string UserEmail { get; set; } = string.Empty;

        public IList<ContactReplyItemViewModel> Messages { get; set; } = new List<ContactReplyItemViewModel>();
    }

    public class ContactReplyItemViewModel
    {
        public int ContactMessageId { get; set; }

        public string? Subject { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? AdminReply { get; set; }

        public ContactMessageStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RepliedAt { get; set; }
    }
}
