﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using OperationCHAN.Data;
using OperationCHAN.Models;

namespace OperationCHAN.Hubs;

public class SettingsHub : Hub
{
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;
    public SettingsHub(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }
    
    /// <summary>
    /// Sets or removes the user as studass
    /// </summary>
    /// <param name="userName">The username of the user</param>
    /// <param name="courseCode">The course code to remove or add as studass to</param>
    /// <param name="setStudass">Booelan representing whether to set or remove as studass</param>
    public async Task SetStudass(string userName, string courseCode, bool setStudass)
    {
        ApplicationUser user = _db.Users.First(user => user.Nickname == userName);
        
        // A user cant be both an admin and a studass, so return if already admin
        if (setStudass)
        {
            if (user.Role == "admin")
            {
                return;
            }
            
            // Set as studass
            _db.Studas.Add(new Studas(user, courseCode));
            user.Role = "studass";
        }
        else
        {
            // Get the studass data from the studass
            Studas studas = _db.Studas.First(studas => studas.ApplicationUserId == user.Id 
                                                       && studas.Course == courseCode);
            
            // Remove the user as studass
            _db.Studas.Remove(studas);
            var otherCourses = _db.Studas.Where(s => s.ApplicationUserId == user.Id
                                                && s.Course != courseCode).ToList();
            
            // If the user is not studass in other courses, remove the studass role
            if (otherCourses.Count <= 0)
            {
                user.Role = "user";
            }
        }
        await _db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Set a user as admin
    /// </summary>
    /// <param name="userName">The username</param>
    /// <param name="setAdmin">Bool representing whether or not to set the user as admin</param>
    public async Task SetAdmin(string userName, bool setAdmin)
    {
        // Get the user by username
        ApplicationUser user = _db.Users.First(user => user.Nickname == userName);
        if (setAdmin)
        {
            // If the current role is studass, remove all courses
            if (user.Role == "studass")
            {
                var courses = _db.Studas.Where(s => s.ApplicationUserId == user.Id).ToList();
                foreach (var course in courses)
                {
                    _db.Studas.Remove(course);
                }
            }
            
            // Set role
            await _um.AddToRoleAsync(user, "Admin");
            user.Role = "admin";
        }
        else
        {
            // Check how many admins there are 
            var adminRoleId = _db.Roles.First(role => role.Name == "Admin").Id;
            var admins = _db.UserRoles.Select(role => role.RoleId == adminRoleId).ToList();

            if (admins.Count <= 1)
            {
                await SetError("You are the only admin, and thus cannot be removed. Set a new admin first");
            }
            else
            {
                // Remove roles, and set them to user
                await _um.RemoveFromRoleAsync(user, "Admin");
                user.Role = "user";
            }
        }
        
        // Save changes
        await _db.SaveChangesAsync();
        // Update the client
        await GetUserData(userName);
    }
    
    /// <summary>
    /// Get user courses and admin status from database and send it to the client
    /// </summary>
    /// <param name="userName">The username to get data from</param>
    public async Task GetUserData(string userName)
    {
        // Get user data
        var user = _db.Users.First(user => user.Nickname == userName);
        // Check if the user is admin
        bool isAdmin = user.Role.Equals("admin");
        // Get the users courses
        var courses = _db.Studas.Where(studass => studass.ApplicationUserId == user.Id)
            .Select(studass => studass.Course).ToList();
        // Send the data to the client
        await Clients.Caller.SendAsync("ShowStudent", courses, isAdmin);
    }

    public async Task SetError(string error)
    {
        await Clients.Caller.SendAsync("ShowError", error);
    }
}