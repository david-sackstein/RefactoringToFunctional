﻿using FunctionalExtensions;
using SuperMarket.Entities;

namespace SuperMarket.Service
{
    public interface IProductRepository
    {
        void Add(Product product);
        Maybe<Product> Find(int productId);
        void Commit();
    }
}