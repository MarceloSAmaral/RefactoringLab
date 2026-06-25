
/*
 * I know, there is only one integration test so far (and I don't expect to create more).
 * Nonetheless, tests that share a mutable external resource (like a database) should not run in parallel,
 * so I disable parallelization here.
 */
[assembly: CollectionBehavior(DisableTestParallelization = true)]