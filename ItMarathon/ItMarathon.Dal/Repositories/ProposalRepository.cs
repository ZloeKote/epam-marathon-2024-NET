using ItMarathon.Dal.Common.Contracts;
using ItMarathon.Dal.Context;
using ItMarathon.Dal.Entities;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace ItMarathon.Dal.Repositories;

public class ProposalRepository(ApplicationDbContext repositoryContext) :
    RepositoryBase<Proposal>(repositoryContext), IProposalRepository
{
    public async Task<(IEnumerable<Proposal>, long totalCount)> GetProposalsAsync(bool trackChanges, ODataQueryOptions queryOptions)
    {
        IQueryable<Proposal> query = FindAll(trackChanges);

        // filter 
        if (queryOptions.Filter != null)
            query = (IQueryable<Proposal>)queryOptions.Filter.ApplyTo(query, new ODataQuerySettings());

        // sort
        if (queryOptions.OrderBy != null && queryOptions.OrderBy.OrderByClause != null)
        {
            query = queryOptions.OrderBy.ApplyTo(query, new ODataQuerySettings());
        }
        else
        {
            // sort by createdOn if there is no sort option in the request
            query = query.OrderByDescending(p => p.CreatedOn);
        }

        // skip
        if (queryOptions.Skip != null)
            query = queryOptions.Skip.ApplyTo(query, new ODataQuerySettings());

        // top
        if (queryOptions.Top != null)
            query = queryOptions.Top.ApplyTo(query, new ODataQuerySettings());

        // count amount of proposals
        long count = await query.LongCountAsync();
        
        query = query
            .Include(p => p.AppUser)
            .Include(p => p.Photos)
            .Include(p => p.Properties!)
                .ThenInclude(properties => properties.PropertyDefinition)
            .Include(p => p.Properties!)
                .ThenInclude(properties => properties.PredefinedValue)
                    .ThenInclude(prop => prop!.ParentPropertyValue);
        
        var proposals = await query.ToListAsync();

        return (proposals, count);
    }

    public async Task<Proposal?> GetProposalAsync(long proposalId, bool trackChanges)
        => await FindByCondition(c => c.Id.Equals(proposalId), trackChanges)
        .Include(p => p.AppUser)
        .Include(p => p.Photos)
        .Include(p => p.Properties!)
            .ThenInclude(properties => properties.PropertyDefinition)
        .Include(p => p.Properties!)
            .ThenInclude(properties => properties.PredefinedValue)
                .ThenInclude(prop => prop!.ParentPropertyValue)
        .SingleOrDefaultAsync();

    public void CreateProposal(Proposal proposal) => Create(proposal);

    public void DeleteProposal(Proposal proposal) => Delete(proposal);
}
