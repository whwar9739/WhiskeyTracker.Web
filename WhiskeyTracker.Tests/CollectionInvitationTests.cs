using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using WhiskeyTracker.Web.Pages.Collections;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace WhiskeyTracker.Tests;

public class CollectionInvitationTests : TestBase
{
    [Fact]
    public async Task InviteModel_OnPost_CreatesInvitation_WhenValid()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var ownerId = "owner-id";
        var inviteeEmail = "invitee@example.com";
        
        var collection = new Collection { Name = "My Collection" };
        context.Collections.Add(collection);
        context.CollectionMembers.Add(new CollectionMember { Collection = collection, UserId = ownerId, Role = CollectionRole.Owner });
        await context.SaveChangesAsync();

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(ownerId);
        mockUserManager.Setup(um => um.FindByEmailAsync(inviteeEmail)).ReturnsAsync((ApplicationUser?)null);

        var pageModel = new InviteModel(context, mockUserManager.Object)
        {
            Input = new InviteModel.InputModel { Email = inviteeEmail, Role = CollectionRole.Editor }
        };
        SetMockUser(pageModel, ownerId);

        // ACT
        var result = await pageModel.OnPostAsync(collection.Id);

        // ASSERT
        Assert.IsType<RedirectToPageResult>(result);
        var invitation = await context.CollectionInvitations.FirstOrDefaultAsync(i => i.InviteeEmail == inviteeEmail);
        Assert.NotNull(invitation);
        Assert.Equal(CollectionRole.Editor, invitation.Role);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
    }

    [Fact]
    public async Task IndexModel_AcceptInvite_CreatesMembership()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var ownerId = "owner-id";
        var inviteeId = "invitee-id";
        var inviteeEmail = "invitee@example.com";

        var owner = new ApplicationUser { Id = ownerId, Email = "owner@example.com", UserName = "owner" };
        var invitee = new ApplicationUser { Id = inviteeId, Email = inviteeEmail, UserName = "invitee" };
        context.Users.AddRange(owner, invitee);

        var collection = new Collection { Name = "Shared Collection" };
        context.Collections.Add(collection);
        
        var invite = new CollectionInvitation
        {
            Collection = collection,
            InviterUserId = ownerId,
            InviteeEmail = inviteeEmail,
            Role = CollectionRole.Editor,
            Status = InvitationStatus.Pending
        };
        context.CollectionInvitations.Add(invite);
        await context.SaveChangesAsync();

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(invitee);

        var pageModel = new IndexModel(context, mockUserManager.Object);
        SetMockUser(pageModel, inviteeId);

        // ACT
        var result = await pageModel.OnPostAcceptInviteAsync(invite.Id);

        // ASSERT
        Assert.IsType<RedirectToPageResult>(result);
        var membership = await context.CollectionMembers.FirstOrDefaultAsync(m => m.CollectionId == collection.Id && m.UserId == inviteeId);
        Assert.NotNull(membership);
        Assert.Equal(CollectionRole.Editor, membership.Role);
        
        var updatedInvite = await context.CollectionInvitations.FindAsync(invite.Id);
        Assert.Equal(InvitationStatus.Accepted, updatedInvite!.Status);
    }

    [Fact]
    public async Task DetailsModel_RestrictsAccess_ToNonMembers()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var memberId = "member-id";
        var nonMemberId = "non-member-id";

        var collection = new Collection { Id = 1, Name = "Private Collection" };
        context.Collections.Add(collection);
        context.CollectionMembers.Add(new CollectionMember { CollectionId = 1, UserId = memberId });
        await context.SaveChangesAsync();

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(nonMemberId);

        var pageModel = new DetailsModel(context, mockUserManager.Object);
        SetMockUser(pageModel, nonMemberId);

        // ACT
        var result = await pageModel.OnGetAsync(1);

        // ASSERT
        Assert.IsType<ForbidResult>(result);
    }
    [Fact]
    public async Task InviteModel_OnPost_Fails_WhenMemberAlreadyExists()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var ownerId = "owner-id";
        var inviteeId = "invitee-id";
        var inviteeEmail = "invitee@example.com";
        
        var collection = new Collection { Id = 1, Name = "My Collection" };
        context.Collections.Add(collection);
        context.CollectionMembers.Add(new CollectionMember { CollectionId = 1, UserId = ownerId, Role = CollectionRole.Owner });
        context.CollectionMembers.Add(new CollectionMember { CollectionId = 1, UserId = inviteeId, Role = CollectionRole.Viewer });
        
        var invitee = new ApplicationUser { Id = inviteeId, Email = inviteeEmail };
        context.Users.Add(invitee);
        await context.SaveChangesAsync();

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(ownerId);
        mockUserManager.Setup(um => um.FindByEmailAsync(inviteeEmail)).ReturnsAsync(invitee);

        var pageModel = new InviteModel(context, mockUserManager.Object)
        {
            Input = new InviteModel.InputModel { Email = inviteeEmail, Role = CollectionRole.Editor }
        };
        SetMockUser(pageModel, ownerId);

        // ACT
        var result = await pageModel.OnPostAsync(1);

        // ASSERT
        Assert.IsType<PageResult>(result);
        Assert.True(pageModel.ModelState.ContainsKey("Input.Email"));
    }

    [Fact]
    public async Task IndexModel_DeclineInvite_UpdatesStatus()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var inviteeId = "invitee-id";
        var inviteeEmail = "invitee@example.com";

        var invitee = new ApplicationUser { Id = inviteeId, Email = inviteeEmail };
        context.Users.Add(invitee);

        var invite = new CollectionInvitation
        {
            CollectionId = 1,
            InviteeEmail = inviteeEmail,
            Status = InvitationStatus.Pending
        };
        context.CollectionInvitations.Add(invite);
        await context.SaveChangesAsync();

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(invitee);

        var pageModel = new IndexModel(context, mockUserManager.Object);
        SetMockUser(pageModel, inviteeId);

        // ACT
        var result = await pageModel.OnPostDeclineInviteAsync(invite.Id);

        // ASSERT
        Assert.IsType<RedirectToPageResult>(result);
        var updatedInvite = await context.CollectionInvitations.FindAsync(invite.Id);
        Assert.Equal(InvitationStatus.Declined, updatedInvite!.Status);
        
        // Ensure no membership was created
        var membership = await context.CollectionMembers.AnyAsync(m => m.CollectionId == 1 && m.UserId == inviteeId);
        Assert.False(membership);
    }

    [Fact]
    public async Task DetailsModel_LoadsInvitations_ForOwner()
    {
        // ARRANGE
        using var context = GetInMemoryContext();
        var ownerId = "owner-id";

        var collection = new Collection { Id = 1, Name = "My Collection" };
        context.Collections.Add(collection);
        context.CollectionMembers.Add(new CollectionMember { CollectionId = 1, UserId = ownerId, Role = CollectionRole.Owner });
        
        context.CollectionInvitations.Add(new CollectionInvitation 
        { 
            CollectionId = 1, 
            InviterUserId = ownerId, 
            InviteeEmail = "test@example.com", 
            Status = InvitationStatus.Pending 
        });
        await context.SaveChangesAsync();

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(ownerId);

        var pageModel = new DetailsModel(context, mockUserManager.Object);
        SetMockUser(pageModel, ownerId);

        // ACT
        await pageModel.OnGetAsync(1);

        // ASSERT
        Assert.True(pageModel.IsOwner);
        Assert.Single(pageModel.PendingInvitations);
    }
}
