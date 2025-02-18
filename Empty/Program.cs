using CVRPTW;

var list = new IDerived[2];

Any(list);

static bool Any(IBase[] list) => list.Any();

interface IBase { }

interface IDerived : IBase { }

