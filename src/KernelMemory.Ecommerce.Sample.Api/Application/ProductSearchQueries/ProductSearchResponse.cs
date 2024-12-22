﻿using KernelMemory.Ecommerce.Sample.Api.Domain;

namespace KernelMemory.Ecommerce.Sample.Api.Application.ProductSearchQueries;

public sealed record ProductSearchResponse(
    bool NoResult,
    double MinRelevance,
    int RelevantSourcesCount,
    IReadOnlyCollection<Product> Products);

