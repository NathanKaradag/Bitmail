using System;
using System.Collections.Generic;
using System.Text;
using Bitmail.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bitmail.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //Sets the relation between Organisation and Contact with their foreignkeys
            builder.Entity<OrganisationContact>().HasKey(oc => new { oc.OrganisationId, oc.ContactId });
            builder.Entity<OrganisationContact>().HasOne(oc => oc.Organisation).WithMany(o => o.OrganisationContacts).HasForeignKey(oc => oc.OrganisationId);
            builder.Entity<OrganisationContact>().HasOne(oc => oc.Contact).WithMany(c => c.OrganisationContacts).HasForeignKey(oc => oc.ContactId);

            //Sets the relation between Contact and Tag with their foreignkeys
            builder.Entity<ContactTag>().HasKey(ct => new { ct.ContactId, ct.TagId });
            builder.Entity<ContactTag>().HasOne(ct => ct.Contact).WithMany(c => c.ContactTags).HasForeignKey(ct => ct.ContactId);
            builder.Entity<ContactTag>().HasOne(ct => ct.Tag).WithMany(t => t.ContactTags).HasForeignKey(oc => oc.TagId);
        }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactTag> ContactTags { get; set; }
        public DbSet<OrganisationContact> OrganisationContacts { get; set; }
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}