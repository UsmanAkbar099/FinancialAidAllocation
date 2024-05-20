﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FinancialAidAllocation.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class FAAToolEntities : DbContext
    {
        public FAAToolEntities()
            : base("name=FAAToolEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Amount> Amounts { get; set; }
        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<Budget> Budgets { get; set; }
        public virtual DbSet<Committee> Committees { get; set; }
        public virtual DbSet<Criterion> Criteria { get; set; }
        public virtual DbSet<EvidenceDocument> EvidenceDocuments { get; set; }
        public virtual DbSet<Faculty> Faculties { get; set; }
        public virtual DbSet<FeedBack> FeedBacks { get; set; }
        public virtual DbSet<FinancialAid> FinancialAids { get; set; }
        public virtual DbSet<Grader> Graders { get; set; }
        public virtual DbSet<MeritBase> MeritBases { get; set; }
        public virtual DbSet<Policy> Policies { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Suggestion> Suggestions { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
