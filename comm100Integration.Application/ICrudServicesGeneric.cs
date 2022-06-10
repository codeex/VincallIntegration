using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Comm100Integration.Application
{
    public interface ICrudServices<TContext> : ICrudServices where TContext : DbContext { }
}
