using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyFlow.Api.Auth;
using MoneyFlow.Api.Common;
using MoneyFlow.Api.Dtos;
using MoneyFlow.Api.Services;

namespace MoneyFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/months")]
public sealed class MonthsController(MonthsService months, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<MonthListItemDto>>>> List()
    {
        var data = await months.ListAsync(currentUser.UserId);
        return Ok(ApiResponse<IEnumerable<MonthListItemDto>>.Ok(data, new { count = data.Count() }));
    }

    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<MonthSummaryDto>>> Current() =>
        Ok(ApiResponse<MonthSummaryDto>.Ok(await months.GetCurrentAsync(currentUser.UserId)));

    [HttpGet("{year:int}/{month:int}")]
    public async Task<ActionResult<ApiResponse<MonthSummaryDto>>> Get(int year, int month) =>
        Ok(ApiResponse<MonthSummaryDto>.Ok(await months.GetByYearMonthAsync(currentUser.UserId, year, month)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MonthSummaryDto>>> Create(CreateMonthRequest request)
    {
        var data = await months.CreateAsync(currentUser.UserId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MonthSummaryDto>.Ok(data));
    }

    [HttpGet("{monthlyPeriodId:int}/summary")]
    public async Task<ActionResult<ApiResponse<MonthSummaryDto>>> Summary(int monthlyPeriodId) =>
        Ok(ApiResponse<MonthSummaryDto>.Ok(await months.GetSummaryAsync(currentUser.UserId, monthlyPeriodId)));
}
