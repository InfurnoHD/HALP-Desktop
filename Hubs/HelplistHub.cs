using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;
using NuGet.Protocol;
using OperationCHAN.Data;
using OperationCHAN.Models;

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
        /// <param name="course">The course you are in</param>
        /// <param name="nickname">The nickname to show</param>
        /// <param name="description">The description to show</param>
        /// <param name="room">The room to show</param>
        public async Task AddToHelplist(int ticketID, string course, string nickname, string description, string room)
        {
            await Clients.Groups(course).SendAsync("AddToHelplist", ticketID, nickname, description, room);
        }
        public async Task AddToArchive(int ticketID, string course, string nickname, string description, string status, string room)
        {
            await Clients.Groups(course).SendAsync("AddToArchive", ticketID, nickname, description, status, room);
        }
        
        public async Task UpdateHelplist(int ticketID, string course, string nickname, string description, string room)
        {
            await Clients.Groups(course).SendAsync("UpdateHelplist", ticketID, nickname, description, room);
        }

        /// <summary>
        /// Adds an ticket to the archive
        /// </summary>
        /// <param name="ticketID">The ID of the ticket in the database</param>
        public async Task RemoveFromHelplist(int ticketID)
        {
            var ticket = SetTicketStatus(ticketID, "Finished");

            await AddToArchive(ticket.Id, ticket.Course, ticket.Nickname, ticket.Description, ticket.Status, ticket.Room);

            await Clients.Groups(ticket.Course).SendAsync("RemoveFromHelplist", ticketID);
        }

        public async Task GetQueueNumber(int ticketID)
        {
            
        }

        /// <summary>
        /// Removes an ticket from archive, and puts it back into the helplist
        /// </summary>
        /// <param name="ticketID">The ID of the ticket in the database</param>
        public async Task RemoveFromArchive(int ticketID)
        {
            var ticket = SetTicketStatus(ticketID, "Waiting");
            
            await AddToHelplist(ticket.Id, ticket.Course, ticket.Nickname, ticket.Description,  ticket.Room);
            
            await Clients.Groups(ticket.Course).SendAsync("RemoveFromArchive", ticketID);
        }
        
        private HelplistModel SetTicketStatus(int id, string status)
        {
            var ticket = _db.HelpList.First(ticket => ticket.Id == id);
            ticket.Status = status;
            _db.SaveChangesAsync();
            return ticket;
        }
        

        /// <summary>
        /// Add the user to the group
        /// </summary>
        /// <param name="course"></param>
        public async Task AddToGroup(string course)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, course);
        }

        /// <summary>
        /// Remove a user from the group
        /// </summary>
        /// <param name="groupName"></param>
        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        
        public async Task RemovedByUser(int ticketID)
        {
            var ticket = SetTicketStatus(ticketID, "Removed");
            
            await AddToArchive(ticket.Id, ticket.Course, ticket.Nickname, ticket.Description, ticket.Status, ticket.Room);
        }
    }
}