using Xunit;

// The notification tests share App.Logger (a plain List<string>) and create real
// temp directories, so keep the whole suite on a single worker for determinism.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
