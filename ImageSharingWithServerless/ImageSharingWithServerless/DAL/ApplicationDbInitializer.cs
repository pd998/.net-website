﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ImageSharingWithServerless.Models;
using ImageSharingModels;

namespace ImageSharingWithServerless.DAL
{
    public  class ApplicationDbInitializer
    {
        private ApplicationDbContext db;
        private IImageStorage imageStorage;
        private ILogContext logContext;
        private ILogger<ApplicationDbInitializer> logger;
        public ApplicationDbInitializer(ApplicationDbContext db, 
                                        IImageStorage imageStorage,
                                        ILogContext logContext,
                                        ILogger<ApplicationDbInitializer> logger)
        {
            this.db = db;
            this.imageStorage = imageStorage;   
            this.logContext = logContext;
            this.logger = logger;
        }

        public async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            /*
             * Initialize databases.
             */
            logger.LogInformation("Initializing databases and blob storage....");
            await imageStorage.InitImageStorage();

            logger.LogInformation("Clearing out the user database....");
            await db.Database.MigrateAsync();
            db.RemoveRange(db.Users);
            await db.SaveChangesAsync();

            logger.LogDebug("Adding role: User");
            var idResult = await CreateRole(serviceProvider, "User");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create User role!");
            }

            // TODO add other roles

            logger.LogDebug("Adding role: Supervisor");
            idResult = await CreateRole(serviceProvider, "Supervisor");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create Supervisor role!");
            }

            logger.LogDebug("Adding role: Admin");
            idResult = await CreateRole(serviceProvider, "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create Admin role!");
            }

            logger.LogDebug("Adding role: Approver");
            idResult = await CreateRole(serviceProvider, "Approver");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create Approver role!");
            }

            logger.LogDebug("Adding user: jfk");
            idResult = await CreateAccount(serviceProvider, "jfk@example.org", "jfk123", "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create jfk user!");
            }

            logger.LogDebug("Adding user: nixon");
            idResult = await CreateAccount(serviceProvider, "nixon@example.org", "nixon123", "User");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create nixon user!");
            }

            // TODO add other users and assign more roles

            logger.LogDebug("Adding user: user1");
            idResult = await CreateAccount(serviceProvider, "user1@example.org", "user123", "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create user1 user!");
            }

            logger.LogDebug("Adding user: user2");
            idResult = await CreateAccount(serviceProvider, "user2@example.org", "user123", "Supervisor");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create user2 user!");
            }

            logger.LogDebug("Adding user: user3");
            idResult = await CreateAccount(serviceProvider, "user3@example.org", "user123", "Approver");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create user3 user!");
            }

            logger.LogDebug("Adding user: user4");
            idResult = await CreateAccount(serviceProvider, "user4@example.org", "user123", "User");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create user4 user!");
            }

            logger.LogDebug("Adding user: user5");
            idResult = await CreateAccount(serviceProvider, "user5@example.org", "user123", "User");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create user5 user!");
            }

            await db.SaveChangesAsync();

        }

        public static async Task<IdentityResult> CreateRole(IServiceProvider provider,
                                                            string role)
        {
            RoleManager<IdentityRole> roleManager = provider
                .GetRequiredService
                       <RoleManager<IdentityRole>>();
            var idResult = IdentityResult.Success;
            if (await roleManager.FindByNameAsync(role) == null)
            {
                idResult = await roleManager.CreateAsync(new IdentityRole(role));
            }
            return idResult;
        }

        public static async Task<IdentityResult> CreateAccount(IServiceProvider provider,
                                                               string email, 
                                                               string password,
                                                               string role)
        {
            UserManager<ApplicationUser> userManager = provider
                .GetRequiredService
                       <UserManager<ApplicationUser>>();
            var idResult = IdentityResult.Success;

            if (await userManager.FindByNameAsync(email) == null)
            {
                ApplicationUser user = new ApplicationUser { UserName = email, Email = email };
                idResult = await userManager.CreateAsync(user, password);

                if (idResult.Succeeded)
                {
                    idResult = await userManager.AddToRoleAsync(user, role);
                }
            }

            return idResult;
        }

    }
}