using FluentAssertions;
using ItMarathon.Dal.Context;
using ItMarathon.Dal.Entities;
using ItMarathon.Dal.Repositories;
using ItMarathon.Tests.DalTests.Fixtures;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace ItMarathon.Tests.DalTests.RepositoriesTests;

public class ProposalRepositoryTests
{
    [Theory]
    [ProposalFixture]
    public async Task GetAllProposalsAsync_ShouldReturnAllProposals(ApplicationDbContext context, ODataQueryOptions queryOptions)
    {
        // Arrange
        var repository = new ProposalRepository(context);
        var initialCount = await context.Proposals.CountAsync();
        
        // Act
        var (proposals, totalCount) = await repository.GetProposalsAsync(false, queryOptions);

        // Assert
        proposals.Should().NotBeNullOrEmpty();
        totalCount.Should().Be(initialCount);
    }

    [Theory]
    [ProposalFixture]
    public async Task GetProposalAsync_ShouldReturnProposalById(ApplicationDbContext context)
    {
        // Arrange
        var repository = new ProposalRepository(context);
        var existingProposal = await context.Proposals.FirstOrDefaultAsync();

        // Act
        var result = await repository.GetProposalAsync(existingProposal!.Id, false);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(existingProposal.Id);
    }

    [Theory]
    [ProposalFixture]
    public async Task CreateProposal_ShouldAddNewProposalToDatabase(ApplicationDbContext context, Proposal proposalToCreate)
    {
        // Arrange
        var repository = new ProposalRepository(context);
        var initialCount = await context.Proposals.CountAsync();

        // Act
        repository.CreateProposal(proposalToCreate);
        await context.SaveChangesAsync();

        // Assert
        context.Proposals.Should().Contain(proposalToCreate);
        (await context.Proposals.CountAsync()).Should().Be(initialCount + 1);
    }

    [Theory]
    [ProposalFixture]
    public async Task DeleteProposal_ShouldRemoveProposalFromDatabase(ApplicationDbContext context)
    {
        // Arrange
        var repository = new ProposalRepository(context);
        var proposalToDelete = await context.Proposals.FirstAsync();
        var initialCount = await context.Proposals.CountAsync();

        // Act
        repository.DeleteProposal(proposalToDelete);
        await context.SaveChangesAsync();

        // Assert
        context.Proposals.Should().NotContain(proposalToDelete);
        (await context.Proposals.CountAsync()).Should().Be(initialCount - 1);
    }

    [Theory]
    [ProposalFixture]
    public async Task GetProposalAsync_WhenProposalDoesNotExist_ShouldReturnNull(ApplicationDbContext context)
    {
        // Arrange
        var repository = new ProposalRepository(context);
        var nonExistentProposalId = -1L;

        // Act
        var result = await repository.GetProposalAsync(nonExistentProposalId, false);

        // Assert
        result.Should().BeNull();
    }
}