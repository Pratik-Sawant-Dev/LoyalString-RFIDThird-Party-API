using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    public interface IMasterDataService
    {
        // Category Master Operations
        Task<IEnumerable<CategoryMasterDto>> GetAllCategoriesAsync();
        Task<CategoryMasterDto?> GetCategoryByIdAsync(int categoryId);
        Task<CategoryMasterDto> CreateCategoryAsync(CreateCategoryMasterDto createDto);
        Task<CategoryMasterDto> UpdateCategoryAsync(UpdateCategoryMasterDto updateDto);
        Task<bool> DeleteCategoryAsync(int categoryId);

        // Purity Master Operations
        Task<IEnumerable<PurityMasterDto>> GetAllPuritiesAsync();
        Task<PurityMasterDto?> GetPurityByIdAsync(int purityId);
        Task<PurityMasterDto> CreatePurityAsync(CreatePurityMasterDto createDto);
        Task<PurityMasterDto> UpdatePurityAsync(UpdatePurityMasterDto updateDto);
        Task<bool> DeletePurityAsync(int purityId);

        // Design Master Operations
        Task<IEnumerable<DesignMasterDto>> GetAllDesignsAsync();
        Task<DesignMasterDto?> GetDesignByIdAsync(int designId);
        Task<DesignMasterDto> CreateDesignAsync(CreateDesignMasterDto createDto);
        Task<DesignMasterDto> UpdateDesignAsync(UpdateDesignMasterDto updateDto);
        Task<bool> DeleteDesignAsync(int designId);

        // Box Master Operations
        Task<IEnumerable<BoxMasterDto>> GetAllBoxesAsync();
        Task<BoxMasterDto?> GetBoxByIdAsync(int boxId);
        Task<BoxMasterDto> CreateBoxAsync(CreateBoxMasterDto createDto);
        Task<BoxMasterDto> UpdateBoxAsync(UpdateBoxMasterDto updateDto);
        Task<bool> DeleteBoxAsync(int boxId);
        Task<IEnumerable<BoxMasterDto>> GetActiveBoxesAsync();
        Task<IEnumerable<BoxMasterDto>> GetBoxesByTypeAsync(string boxType);

        // Counter Master Operations
        Task<IEnumerable<CounterMasterDto>> GetAllCountersAsync();
        Task<IEnumerable<CounterMasterDto>> GetCountersByClientAsync(string clientCode);
        Task<IEnumerable<CounterMasterDto>> GetCountersByBranchAsync(int branchId);
        Task<CounterMasterDto?> GetCounterByIdAsync(int counterId);
        Task<CounterMasterDto> CreateCounterAsync(CreateCounterMasterDto createDto);
        Task<CounterMasterDto> UpdateCounterAsync(UpdateCounterMasterDto updateDto);
        Task<bool> DeleteCounterAsync(int counterId);

        // Branch Master Operations
        Task<IEnumerable<BranchMasterDto>> GetAllBranchesAsync();
        Task<IEnumerable<BranchMasterDto>> GetBranchesByClientAsync(string clientCode);
        Task<BranchMasterDto?> GetBranchByIdAsync(int branchId);
        Task<BranchMasterDto> CreateBranchAsync(CreateBranchMasterDto createDto);
        Task<BranchMasterDto> UpdateBranchAsync(UpdateBranchMasterDto updateDto);
        Task<bool> DeleteBranchAsync(int branchId);

        // Product Master Operations
        Task<IEnumerable<ProductMasterDto>> GetAllProductsAsync();
        Task<ProductMasterDto?> GetProductByIdAsync(int productId);
        Task<ProductMasterDto> CreateProductAsync(CreateProductMasterDto createDto);
        Task<ProductMasterDto> UpdateProductAsync(UpdateProductMasterDto updateDto);
        Task<bool> DeleteProductAsync(int productId);

        // Master Data Summary Operations
        Task<MasterDataSummaryDto> GetMasterDataSummaryAsync();
        Task<IEnumerable<MasterDataCountsDto>> GetMasterDataCountsAsync();
        Task<MasterDataSummaryDto> GetMasterDataSummaryByClientAsync(string clientCode);
    }
}
