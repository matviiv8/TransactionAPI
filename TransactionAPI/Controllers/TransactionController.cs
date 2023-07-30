using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Infrastructure.Interfaces;
using TransactionAPI.Infrastructure.ViewModels.Transactions;

namespace TransactionAPI.Controllers
{
    /// <summary>
    /// Controller for processing transactions.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/transaction")]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            this._transactionService = transactionService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves a list of transactions filtered by the specified criteria.
        /// </summary>
        /// <param name="filter">The filter criteria for retrieving transactions.</param>
        /// <returns>A collection of transactions that match the filter criteria.</returns>
        /// <response code="200">Returns the collection of transactions that match the filter criteria.</response>
        /// <response code="401">Unauthorized. User must be authenticated to access this endpoint.</response>
        /// <response code="404">No transactions found that match the specified filter criteria.</response>
        /// <response code="500">An error occurred while retrieving the transactions.</response>
        [HttpGet("alltransactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] TransactionFilterViewModel filter)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsByFilter(filter.Types, filter.Status, filter.ClientName);

                if(transactions.Count() == 0)
                {
                    return NotFound("Transactions not found.");
                }

                return Ok(transactions);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in TransactionController.GetAll(TransactionFilterViewModel filter): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");
                _logger.LogTrace(exception.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        /// <summary>
        /// Updates the status of a transaction by its ID.
        /// </summary>
        /// <param name="id">The ID of the transaction to update.</param>
        /// <param name="status">The new status for the transaction.</param>
        /// <returns>The updated transaction with the new status.</returns>
        /// <response code="200">Returns the updated transaction with the new status.</response>
        /// <response code="400">Status is required or invalid.</response>
        /// <response code="401">Unauthorized. User must be authenticated to access this endpoint.</response>
        /// <response code="404">Transaction not found.</response>
        /// <response code="500">An error occurred while updating the transaction status.</response>
        [HttpGet("updatetransactionstatus/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTransactionStatusById([FromRoute] int id, [FromQuery] Status? status)
        {
            try
            {
                if(status == null)
                {
                    return BadRequest("Status is required.");
                }

                var transaction = await _transactionService.GetTransactionById(id);

                if(transaction == null)
                { 
                    return NotFound("Transaction not found.");
                }

                var updatedTransaction = await _transactionService.UpdateTransactionStatus(transaction, status);

                return Ok(updatedTransaction);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error in TransactionController.UpdateTransactionStatusById(int id, string status): {exception.Message}");
                _logger.LogError($"Inner exception:\n{exception.InnerException}");
                _logger.LogTrace(exception.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
