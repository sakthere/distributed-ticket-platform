using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Policies
{
    // The ticket lifecycle as a state machine. Open and Assigned/InProgress can
    // all be rejected (a ticket can turn out to be invalid at any point before
    // it's actually resolved); Resolved can only move forward to Closed; Closed
    // and Rejected are both terminal - nothing transitions out of them.
    //
    // Deliberately not exposing "reopen" (Resolved/Closed/Rejected -> anything)
    // yet - no story has asked for it, and adding it later is a one-line change
    // to the table below, not a redesign.
    public static class TicketStatusPolicy
    {
        private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
        {
            [TicketStatus.Open] = new[] { TicketStatus.Assigned, TicketStatus.Rejected },
            [TicketStatus.Assigned] = new[] { TicketStatus.InProgress, TicketStatus.Rejected },
            [TicketStatus.InProgress] = new[] { TicketStatus.Resolved, TicketStatus.Rejected },
            [TicketStatus.Resolved] = new[] { TicketStatus.Closed },
            [TicketStatus.Closed] = Array.Empty<TicketStatus>(),
            [TicketStatus.Rejected] = Array.Empty<TicketStatus>()
        };

        public static bool IsValidTransition(TicketStatus from, TicketStatus to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
        }
    }
}
