using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterDataService _masterDataService;
        private readonly IActivityLoggingService _activityLoggingService;

        public MasterDataController(IMasterDataService masterDataService, IActivityLoggingService activityLoggingService)
        {
            _masterDataService = masterDataService;
            _activityLoggingService = activityLoggingService;
        }

        #region Category Master Endpoints

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _masterDataService.GetAllCategoriesAsync();
                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("categories/{categoryId}")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            try
            {
                var category = await _masterDataService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                    return NotFound(new { success = false, message = "Category not found" });

                return Ok(new { success = true, data = category });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new category
        /// </summary>
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryMasterDto createDto)
        {
            try
            {
                var category = await _masterDataService.CreateCategoryAsync(createDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Category", "Create", 
                    $"Created category: {category.CategoryName}", "tblCategoryMaster", category.CategoryId);

                return CreatedAtAction(nameof(GetCategoryById), new { categoryId = category.CategoryId }, 
                    new { success = true, data = category });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update category
        /// </summary>
        [HttpPut("categories")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryMasterDto updateDto)
        {
            try
            {
                var category = await _masterDataService.UpdateCategoryAsync(updateDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Category", "Update", 
                    $"Updated category: {category.CategoryName}", "tblCategoryMaster", category.CategoryId);

                return Ok(new { success = true, data = category });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete category
        /// </summary>
        [HttpDelete("categories/{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var result = await _masterDataService.DeleteCategoryAsync(categoryId);
                if (!result)
                    return NotFound(new { success = false, message = "Category not found" });

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Category", "Delete", 
                    $"Deleted category with ID: {categoryId}", "tblCategoryMaster", categoryId);

                return Ok(new { success = true, message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Purity Master Endpoints

        /// <summary>
        /// Get all purities
        /// </summary>
        [HttpGet("purities")]
        public async Task<IActionResult> GetAllPurities()
        {
            try
            {
                var purities = await _masterDataService.GetAllPuritiesAsync();
                return Ok(new { success = true, data = purities });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get purity by ID
        /// </summary>
        [HttpGet("purities/{purityId}")]
        public async Task<IActionResult> GetPurityById(int purityId)
        {
            try
            {
                var purity = await _masterDataService.GetPurityByIdAsync(purityId);
                if (purity == null)
                    return NotFound(new { success = false, message = "Purity not found" });

                return Ok(new { success = true, data = purity });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new purity
        /// </summary>
        [HttpPost("purities")]
        public async Task<IActionResult> CreatePurity([FromBody] CreatePurityMasterDto createDto)
        {
            try
            {
                var purity = await _masterDataService.CreatePurityAsync(createDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Purity", "Create", 
                    $"Created purity: {purity.PurityName}", "tblPurityMaster", purity.PurityId);

                return CreatedAtAction(nameof(GetPurityById), new { purityId = purity.PurityId }, 
                    new { success = true, data = purity });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update purity
        /// </summary>
        [HttpPut("purities")]
        public async Task<IActionResult> UpdatePurity([FromBody] UpdatePurityMasterDto updateDto)
        {
            try
            {
                var purity = await _masterDataService.UpdatePurityAsync(updateDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Purity", "Update", 
                    $"Updated purity: {purity.PurityName}", "tblPurityMaster", purity.PurityId);

                return Ok(new { success = true, data = purity });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete purity
        /// </summary>
        [HttpDelete("purities/{purityId}")]
        public async Task<IActionResult> DeletePurity(int purityId)
        {
            try
            {
                var result = await _masterDataService.DeletePurityAsync(purityId);
                if (!result)
                    return NotFound(new { success = false, message = "Purity not found" });

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Purity", "Delete", 
                    $"Deleted purity with ID: {purityId}", "tblPurityMaster", purityId);

                return Ok(new { success = true, message = "Purity deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Design Master Endpoints

        /// <summary>
        /// Get all designs
        /// </summary>
        [HttpGet("designs")]
        public async Task<IActionResult> GetAllDesigns()
        {
            try
            {
                var designs = await _masterDataService.GetAllDesignsAsync();
                return Ok(new { success = true, data = designs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get design by ID
        /// </summary>
        [HttpGet("designs/{designId}")]
        public async Task<IActionResult> GetDesignById(int designId)
        {
            try
            {
                var design = await _masterDataService.GetDesignByIdAsync(designId);
                if (design == null)
                    return NotFound(new { success = false, message = "Design not found" });

                return Ok(new { success = true, data = design });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new design
        /// </summary>
        [HttpPost("designs")]
        public async Task<IActionResult> CreateDesign([FromBody] CreateDesignMasterDto createDto)
        {
            try
            {
                var design = await _masterDataService.CreateDesignAsync(createDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Design", "Create", 
                    $"Created design: {design.DesignName}", "tblDesignMaster", design.DesignId);

                return CreatedAtAction(nameof(GetDesignById), new { designId = design.DesignId }, 
                    new { success = true, data = design });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update design
        /// </summary>
        [HttpPut("designs")]
        public async Task<IActionResult> UpdateDesign([FromBody] UpdateDesignMasterDto updateDto)
        {
            try
            {
                var design = await _masterDataService.UpdateDesignAsync(updateDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Design", "Update", 
                    $"Updated design: {design.DesignName}", "tblDesignMaster", design.DesignId);

                return Ok(new { success = true, data = design });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete design
        /// </summary>
        [HttpDelete("designs/{designId}")]
        public async Task<IActionResult> DeleteDesign(int designId)
        {
            try
            {
                var result = await _masterDataService.DeleteDesignAsync(designId);
                if (!result)
                    return NotFound(new { success = false, message = "Design not found" });

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Design", "Delete", 
                    $"Deleted design with ID: {designId}", "tblDesignMaster", designId);

                return Ok(new { success = true, message = "Design deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Box Master Endpoints

        /// <summary>
        /// Get all boxes
        /// </summary>
        [HttpGet("boxes")]
        public async Task<IActionResult> GetAllBoxes()
        {
            try
            {
                var boxes = await _masterDataService.GetAllBoxesAsync();
                return Ok(new { success = true, data = boxes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get active boxes only
        /// </summary>
        [HttpGet("boxes/active")]
        public async Task<IActionResult> GetActiveBoxes()
        {
            try
            {
                var boxes = await _masterDataService.GetActiveBoxesAsync();
                return Ok(new { success = true, data = boxes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get boxes by type
        /// </summary>
        [HttpGet("boxes/type/{boxType}")]
        public async Task<IActionResult> GetBoxesByType(string boxType)
        {
            try
            {
                var boxes = await _masterDataService.GetBoxesByTypeAsync(boxType);
                return Ok(new { success = true, data = boxes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get box by ID
        /// </summary>
        [HttpGet("boxes/{boxId}")]
        public async Task<IActionResult> GetBoxById(int boxId)
        {
            try
            {
                var box = await _masterDataService.GetBoxByIdAsync(boxId);
                if (box == null)
                    return NotFound(new { success = false, message = "Box not found" });

                return Ok(new { success = true, data = box });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new box
        /// </summary>
        [HttpPost("boxes")]
        public async Task<IActionResult> CreateBox([FromBody] CreateBoxMasterDto createDto)
        {
            try
            {
                var box = await _masterDataService.CreateBoxAsync(createDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Box", "Create", 
                    $"Created box: {box.BoxName}", "tblBoxMaster", box.BoxId);

                return CreatedAtAction(nameof(GetBoxById), new { boxId = box.BoxId }, 
                    new { success = true, data = box });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update box
        /// </summary>
        [HttpPut("boxes")]
        public async Task<IActionResult> UpdateBox([FromBody] UpdateBoxMasterDto updateDto)
        {
            try
            {
                var box = await _masterDataService.UpdateBoxAsync(updateDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Box", "Update", 
                    $"Updated box: {box.BoxName}", "tblBoxMaster", box.BoxId);

                return Ok(new { success = true, data = box });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete box
        /// </summary>
        [HttpDelete("boxes/{boxId}")]
        public async Task<IActionResult> DeleteBox(int boxId)
        {
            try
            {
                var result = await _masterDataService.DeleteBoxAsync(boxId);
                if (!result)
                    return NotFound(new { success = false, message = "Box not found" });

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Box", "Delete", 
                    $"Deleted box with ID: {boxId}", "tblBoxMaster", boxId);

                return Ok(new { success = true, message = "Box deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Counter Master Endpoints

        /// <summary>
        /// Get all counters
        /// </summary>
        [HttpGet("counters")]
        public async Task<IActionResult> GetAllCounters()
        {
            try
            {
                var counters = await _masterDataService.GetAllCountersAsync();
                return Ok(new { success = true, data = counters });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get counters by client
        /// </summary>
        [HttpGet("counters/client/{clientCode}")]
        public async Task<IActionResult> GetCountersByClient(string clientCode)
        {
            try
            {
                var counters = await _masterDataService.GetCountersByClientAsync(clientCode);
                return Ok(new { success = true, data = counters });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get counters by branch
        /// </summary>
        [HttpGet("counters/branch/{branchId}")]
        public async Task<IActionResult> GetCountersByBranch(int branchId)
        {
            try
            {
                var counters = await _masterDataService.GetCountersByBranchAsync(branchId);
                return Ok(new { success = true, data = counters });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get counter by ID
        /// </summary>
        [HttpGet("counters/{counterId}")]
        public async Task<IActionResult> GetCounterById(int counterId)
        {
            try
            {
                var counter = await _masterDataService.GetCounterByIdAsync(counterId);
                if (counter == null)
                    return NotFound(new { success = false, message = "Counter not found" });

                return Ok(new { success = true, data = counter });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new counter
        /// </summary>
        [HttpPost("counters")]
        public async Task<IActionResult> CreateCounter([FromBody] CreateCounterMasterDto createDto)
        {
            try
            {
                var counter = await _masterDataService.CreateCounterAsync(createDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Counter", "Create", 
                    $"Created counter: {counter.CounterName}", "tblCounterMaster", counter.CounterId);

                return CreatedAtAction(nameof(GetCounterById), new { counterId = counter.CounterId }, 
                    new { success = true, data = counter });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update counter
        /// </summary>
        [HttpPut("counters")]
        public async Task<IActionResult> UpdateCounter([FromBody] UpdateCounterMasterDto updateDto)
        {
            try
            {
                var counter = await _masterDataService.UpdateCounterAsync(updateDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Counter", "Update", 
                    $"Updated counter: {counter.CounterName}", "tblCounterMaster", counter.CounterId);

                return Ok(new { success = true, data = counter });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete counter
        /// </summary>
        [HttpDelete("counters/{counterId}")]
        public async Task<IActionResult> DeleteCounter(int counterId)
        {
            try
            {
                var result = await _masterDataService.DeleteCounterAsync(counterId);
                if (!result)
                    return NotFound(new { success = false, message = "Counter not found" });

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Counter", "Delete", 
                    $"Deleted counter with ID: {counterId}", "tblCounterMaster", counterId);

                return Ok(new { success = true, message = "Counter deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Branch Master Endpoints

        /// <summary>
        /// Get all branches
        /// </summary>
        [HttpGet("branches")]
        public async Task<IActionResult> GetAllBranches()
        {
            try
            {
                var branches = await _masterDataService.GetAllBranchesAsync();
                return Ok(new { success = true, data = branches });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get branches by client
        /// </summary>
        [HttpGet("branches/client/{clientCode}")]
        public async Task<IActionResult> GetBranchesByClient(string clientCode)
        {
            try
            {
                var branches = await _masterDataService.GetBranchesByClientAsync(clientCode);
                return Ok(new { success = true, data = branches });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get branch by ID
        /// </summary>
        [HttpGet("branches/{branchId}")]
        public async Task<IActionResult> GetBranchById(int branchId)
        {
            try
            {
                var branch = await _masterDataService.GetBranchByIdAsync(branchId);
                if (branch == null)
                    return NotFound(new { success = false, message = "Branch not found" });

                return Ok(new { success = true, data = branch });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new branch
        /// </summary>
        [HttpPost("branches")]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchMasterDto createDto)
        {
            try
            {
                var branch = await _masterDataService.CreateBranchAsync(createDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Branch", "Create", 
                    $"Created branch: {branch.BranchName}", "tblBranchMaster", branch.BranchId);

                return CreatedAtAction(nameof(GetBranchById), new { branchId = branch.BranchId }, 
                    new { success = true, data = branch });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update branch
        /// </summary>
        [HttpPut("branches")]
        public async Task<IActionResult> UpdateBranch([FromBody] UpdateBranchMasterDto updateDto)
        {
            try
            {
                var branch = await _masterDataService.UpdateBranchAsync(updateDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Branch", "Update", 
                    $"Updated branch: {branch.BranchName}", "tblBranchMaster", branch.BranchId);

                return Ok(new { success = true, data = branch });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete branch
        /// </summary>
        [HttpDelete("branches/{branchId}")]
        public async Task<IActionResult> DeleteBranch(int branchId)
        {
            try
            {
                var result = await _masterDataService.DeleteBranchAsync(branchId);
                if (!result)
                    return NotFound(new { success = false, message = "Branch not found" });

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Branch", "Delete", 
                    $"Deleted branch with ID: {branchId}", "tblBranchMaster", branchId);

                return Ok(new { success = true, message = "Branch deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Product Master Endpoints

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _masterDataService.GetAllProductsAsync();
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("products/{productId}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var product = await _masterDataService.GetProductByIdAsync(productId);
                if (product == null)
                    return NotFound(new { success = false, message = "Product not found" });

                return Ok(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductMasterDto createDto)
        {
            try
            {
                var product = await _masterDataService.CreateProductAsync(createDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Product", "Create", 
                    $"Created product: {product.ProductName}", "tblProductMaster", product.ProductId);

                return CreatedAtAction(nameof(GetProductById), new { productId = product.ProductId }, 
                    new { success = true, data = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update product
        /// </summary>
        [HttpPut("products")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductMasterDto updateDto)
        {
            try
            {
                var product = await _masterDataService.UpdateProductAsync(updateDto);

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Product", "Update", 
                    $"Updated product: {product.ProductName}", "tblProductMaster", product.ProductId);

                return Ok(new { success = true, data = product });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("products/{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                var result = await _masterDataService.DeleteProductAsync(productId);
                if (!result)
                    return NotFound(new { success = false, message = "Product not found" });

                // Log activity
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var clientCode = User.FindFirst("ClientCode")?.Value ?? "";
                await _activityLoggingService.LogActivityAsync(userId, clientCode, "Product", "Delete", 
                    $"Deleted product with ID: {productId}", "tblProductMaster", productId);

                return Ok(new { success = true, message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Master Data Summary Endpoints

        /// <summary>
        /// Get master data summary
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetMasterDataSummary()
        {
            try
            {
                var summary = await _masterDataService.GetMasterDataSummaryAsync();
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get master data counts
        /// </summary>
        [HttpGet("counts")]
        public async Task<IActionResult> GetMasterDataCounts()
        {
            try
            {
                var counts = await _masterDataService.GetMasterDataCountsAsync();
                return Ok(new { success = true, data = counts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get master data summary by client
        /// </summary>
        [HttpGet("summary/client/{clientCode}")]
        public async Task<IActionResult> GetMasterDataSummaryByClient(string clientCode)
        {
            try
            {
                var summary = await _masterDataService.GetMasterDataSummaryByClientAsync(clientCode);
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
