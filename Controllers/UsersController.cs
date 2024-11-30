using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;


namespace MvcMovie.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly MvcMovieContext _context;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsersController(MvcMovieContext context, SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
            _context = context;
        }
        // Block users
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUsers([FromBody] string[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Json(new { success = false, message = "No users selected for blocking." });
            }

            var users = await _context.Users.Where(user => ids.Contains(user.Id)).ToListAsync();
            // Get the current user's ID
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            foreach (var user in users)
            {
                if (!user.IsBlocked) // Block only if not already blocked
                {
                    user.IsBlocked = true; // Set IsBlocked to true
                } 
            }

            await _context.SaveChangesAsync();


            // Check if the current user is in the list of users to be deleted
            if (currentUserId != null && users.Any(user => user.Id == currentUserId))
            {
                // Log out the current user
                await _signInManager.SignOutAsync();

            }

            return Json(new { success = true, message = $"{users.Count} user(s) blocked successfully." });
        }

        // Unblock users
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUsers([FromBody] string[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Json(new { success = false, message = "No users selected for unblocking." });
            }

            var users = await _context.Users.Where(user => ids.Contains(user.Id)).ToListAsync();

            foreach (var user in users)
            {
                if (user.IsBlocked) // Unblock only if blocked
                {
                    user.IsBlocked = false; // Set IsBlocked to false
                }
            }

            

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"{users.Count} user(s) unblocked successfully." });
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IsBlocked,FirstName,LastName,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(applicationUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(applicationUser);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users.FindAsync(id);
            if (applicationUser == null)
            {
                return NotFound();
            }
            return View(applicationUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IsBlocked,FirstName,LastName,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] ApplicationUser applicationUser)
        {
            if (id != applicationUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicationUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationUserExists(applicationUser.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(applicationUser);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: Users/Delete/5
        // POST: Users/DeleteSelected
        [HttpPost]
        // [ValidateAntiForgeryToken]
   public async Task<IActionResult> DeleteSelected([FromBody] string[] ids)
{
    if (ids == null || ids.Length == 0)
    {
        return Json(new { success = false, message = "No users selected for deletion." });
    }

    // Get the current user's ID
    var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    var usersToDelete = _context.Users.Where(user => ids.Contains(user.Id)).ToList();

    if (usersToDelete.Count == 0)
    {
        return Json(new { success = false, message = "No valid users found for deletion." });
    }

    // Check if the current user is in the list of users to be deleted
    if (currentUserId != null && usersToDelete.Any(user => user.Id == currentUserId))
    {
        // Log out the current user
        await _signInManager.SignOutAsync();

    }

    // Remove the selected users from the database
    _context.Users.RemoveRange(usersToDelete);
    await _context.SaveChangesAsync();

    // Handle response differently if the current user was logged out
    if (currentUserId != null && ids.Contains(currentUserId))
    {
        return Json(new { success = true, message = "Your account has been deleted. You have been logged out." });
    }

    return Json(new { success = true, message = $"{usersToDelete.Count} user(s) deleted successfully." });
}

            private bool ApplicationUserExists(string id)
            {
                return _context.Users.Any(e => e.Id == id);
            }
        }


        

}
