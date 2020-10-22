using System.Text.Json;
using api.Helpers;
using Microsoft.AspNetCore.Http;

namespace api.Extensions
{
  public static class HttpExtensions
  {
    public static void AddPaginationHeader(this HttpResponse response,
                                           int currentPage, int itemsPerPage, int totalItems, int totalPages)
    {
      var paginationHeader = new PaginationHeader
      (
        currentPage: currentPage,
        totalPages: totalPages,
        pageSize: itemsPerPage,
        totalCount: totalItems
      );

      var options = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      };

      response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options));
      response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
    }
  }
}
