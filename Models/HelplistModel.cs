﻿using Microsoft.Build.Framework;
using OperationCHAN.Data;

namespace OperationCHAN.Models;

public class HelplistModel
{
    private ApplicationDbContext _db;
    
    public HelplistModel(){}

    public HelplistModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public int Id { get; set; }

    public string Nickname { get; set; } = String.Empty;
    
    public string Room { get; set; } = String.Empty;

    public string Course { get; set; } = String.Empty;

    public string Status { get; set; } = String.Empty;
    
    public string Description { get; set; } = String.Empty;
    
    public List<ApplicationUser> ApplicationUser { get; set; } = new List<ApplicationUser>();

}