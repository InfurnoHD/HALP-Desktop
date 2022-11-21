using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol;
using OperationCHAN.Data;

namespace OperationCHAN.Hubs
{
    public class HelplistHub : Hub
    {
        private ApplicationDbContext _db;
        public HelplistHub(ApplicationDbContext db)
        {
            _db = db;
        }
        
        /// <summary>
        /// Adds an entry to the helplist
        /// </summary>
        /// <param name="ticketID">The ID of the ticket in the database</param>
        /// <param name="nickname">The nickname to show</param>
        /// <param name="description">The description to show</param>
        /// <param name="course">The course you are in</param>
        public async Task AddToHelplist(int ticketID, string course, string nickname, string description)
        {
            await Clients.Groups(course).SendAsync("AddToHelplist", ticketID, nickname, description);
        }
        
        /// <summary>
        /// Removes a ticket from archive
        /// </summary>
        /// <param name="ticketID">The ID of the ticket in the database</param>
        /// <param name="course">The course you are in</param>
        public async Task RemoveFromHelplist(int ticketID, string course)
        {
            await Clients.Groups(course).SendAsync("RemoveFromHelplist", ticketID);
        }
        
        /// <summary>
        /// Adds an ticket to the archive
        /// </summary>
        /// <param name="ticketID">The ID of the ticket in the database</param>
        /// <param name="nickname">The nickname to show</param>
        /// <param name="description">The description to show</param>
        /// <param name="course">The room you are in</param>
        public async Task AddToArchive(int ticketID, string course, string nickname, string description)
        {
            // Remove student from the helplist
            await RemoveFromHelplist(ticketID, course);

            SetTicketStatus(ticketID, "Finished");

            await Clients.Groups(course).SendAsync("AddToArchive", ticketID, nickname, description);
        }

        /// <summary>
        /// Removes an ticket from archive, and puts it back into the helplist
        /// </summary>
        /// <param name="ticketID">The ID of the ticket in the database</param>
        /// <param name="course">The course you are in</param>
        public async Task RemoveFromArchive(int ticketID, string course, string nickname, string description)
        {
            await AddToHelplist(ticketID, course, nickname, description);

            SetTicketStatus(ticketID, "Waiting");
            
            await Clients.Groups(course).SendAsync("RemoveFromArchive", ticketID);
        }
        
        private bool SetTicketStatus(int id, string status)
        {
            var ticket = _db.HelpList.First(ticket => ticket.Id == id);
            ticket.Status = status;
            _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Send a message to a specific group
        /// </summary>
        /// <param name="user"></param>
        /// <param name="nickname"></param>
        /// <param name="description"></param>
        /// <param name="course"></param>
        public async Task SendMessageToGroup(string nickname, string description, string course)
        {
            await Clients.Group(course).SendAsync("UserAdded", nickname, description, course);
        }

        /// <summary>
        /// Add the user to the group
        /// </summary>
        /// <param name="course"></param>
        public async Task AddToGroup(string course)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, course);
            await Clients.Group(course).SendAsync("UserAdded");
        }

        /// <summary>
        /// Remove a user from the group
        /// </summary>
        /// <param name="groupName"></param>
        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            //await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }
        
        public async Task RemovedByUser(int ticketID)
        {
            var ticket = _db.HelpList.Where(ticket => ticket.Id == ticketID);
            Console.WriteLine(ticket);
            // Remove student from the helplist
            await RemoveFromHelplist(ticketID, ticket.First().Course);

            SetTicketStatus(ticketID, "Removed");

            await Clients.Groups(ticket.First().Course).SendAsync("AddToArchive", ticketID, ticket.First().Nickname, ticket.First().Description);
        }
    }
}