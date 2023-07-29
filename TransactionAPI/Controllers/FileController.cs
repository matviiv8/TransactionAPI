using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Transactions;

namespace TransactionAPI.Controllers
{
    /// <summary>
    /// Controller for processing csv files.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/file")]
    public class FileController : Controller
    {
        private readonly IFileService _fileService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, ITransactionService transactionService, ILogger<FileController> logger)
        {
            this._fileService = fileService;
            this._transactionService = transactionService;
            this._logger = logger;
        }

        /// <summary>
        /// Process the uploaded Excel file containing transactions.
        /// </summary>
        /// <param name="file">The Excel file containing transactions.</param>
        /// <returns>A message indicating the success or failure of processing the file.</returns>
        /// <response code="200">The file has been successfully processed and transactions inserted into the database.</response>
        /// <response code="400">The file format is incorrect.</response>
        /// <response code="401">Unauthorized. User must be authenticated to access this endpoint.</response>
        /// <response code="500">An error occurred while processing the file.</response>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessExcelFile(IFormFile file)
        {
            try
            {
                if (!file.FileName.EndsWith(".csv"))
                {
                    return BadRequest("The file format is incorrect.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    await _fileService.ProcessExcelFile(stream);

                    return Ok("The file has been read and inserted into the database.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in FileController.ProcessExcelFile(IFormFile file): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");
                _logger.LogTrace(exception.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        /// <summary>
        /// Export a list of transactions with or without a filter to a file.
        /// </summary>
        /// <param name="filter">The filter criteria for retrieving transactions.</param>
        /// <returns>A message indicating the success or failure of exporting the transactions.</returns>
        /// <response code="200">The export completed successfully. Data saved to the file.</response>
        /// <response code="401">Unauthorized. User must be authenticated to access this endpoint.</response>
        /// <response code="404">No transactions found that match the specified filter criteria.</response>
        /// <response code="500">An error occurred while exporting the transactions.</response>
        [HttpGet("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportTransactionsToCsv([FromQuery] TransactionTypeStatusViewModel filter)
        {
            try
            {
                var types = new List<Domain.Enums.Type>();
                var transactionType = filter.Type as Domain.Enums.Type?;

                if (transactionType != null)
                {
                    types.Add(transactionType.Value);
                }

                var transactions = await _transactionService.GetTransactionsByFilter(types, filter.Status);

                if(transactions.Count == 0)
                {
                    return NotFound("Transactions not found.");
                }

                string filePath = "transactions.csv";
                await _fileService.ExportTransactionsToCsv(transactions, filePath);

                return Ok("Export completed successfully. Data saved to file " + filePath);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in FileController.ExportTransactionsToCsv(TransactionTypeStatusViewModel filter): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");
                _logger.LogTrace(exception.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
