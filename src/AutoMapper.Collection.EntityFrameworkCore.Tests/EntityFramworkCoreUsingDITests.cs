﻿using System;
using AutoMapper.EntityFrameworkCore;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMapper.Collection.EntityFrameworkCore.Tests
{
    public class EntityFramworkCoreUsingDITests : EntityFramworkCoreTestsBase, IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mapper _mapper;
        private readonly IServiceScope _serviceScope;

        public EntityFramworkCoreUsingDITests()
        {
            var services = new ServiceCollection();

            services
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DB>(options => options.UseInMemoryDatabase("EfTestDatabase" + Guid.NewGuid()));

            _serviceProvider = services.BuildServiceProvider();

            _mapper = new Mapper(new MapperConfiguration(x =>
            {
                x.ConstructServicesUsing(type => ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, type));
                x.AddCollectionMappers();
                x.UseEntityFrameworkCoreModel<DB>(_serviceProvider);
                x.CreateMap<ThingDto, Thing>().ReverseMap();
            }));

            _serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }

        public void Dispose()
        {
            _serviceScope?.Dispose();
            _serviceProvider?.Dispose();
        }

        protected override DBContextBase GetDbContext()
        {
            return _serviceScope.ServiceProvider.GetRequiredService<DB>();
        }

        protected override IMapper GetMapper()
        {
            return _mapper;
        }

        public class DB : DBContextBase
        {
            public DB(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }
        }
    }
}
