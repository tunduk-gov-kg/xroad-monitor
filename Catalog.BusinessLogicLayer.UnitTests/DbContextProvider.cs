﻿using System;
using System.Collections.Generic;
using System.Text;
using Catalog.DataAccessLayer;

namespace Catalog.BusinessLogicLayer.UnitTests
{
    internal static class DbContextProvider
    {
        public static AppDbContext RequireDbContext()
        {
            var dbContextFactory = new AppDbContextFactory();
            return dbContextFactory.CreateDbContext(new string[] { });
        }
    }
}
