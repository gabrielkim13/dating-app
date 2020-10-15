using System;

namespace api.Extensions
{
  public static class DateTimeExtensions
  {
    public static int CalculateAge(this DateTime birthDate)
    {
      var today = DateTime.Today;
      var age = today.Year - birthDate.Year;

      if (birthDate.Date > today.AddYears(-age)) age--;

      return age;
    }
  }
}