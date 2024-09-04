using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.AccountReset;

public class AccountReset(ILogger<AccountReset> logger) : WebModule(logger)
{
}
