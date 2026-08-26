# CLogic.Dialogue

The CLogic dialogue system is a node based dialogue system that allows users to
create dialogue using Unity's Graph Toolkit.

> [!WARNING]
> The CLogic Dialogue System is built using Unity's Graph Toolkit, which is still evolving.
Keeping the Dialogue System in Early Access gives us room to adopt new Graph Toolkit capabilities
and adjust the framework as its APIs and workflows mature.
>
> We currently expect the Dialogue System to remain in Early Access until Unity 7,
when Graph Toolkit is expected to be more mature.


The main goal of the CLogic Dialogue System is to allow developers to extend it to their needs
without having to fight the system. As such, many classes are intended to be overriden to accomodate
your project's needs. You are encouraged and expected to extend the system. The list of out-of-the-box
features is deliberately small to avoid making it 'one case fits all' since all projects have different needs.

See [Extending The Dialogue System](https://docs.clogic.dev/manuals/dev.clogic.dialogue/Extending%20The%20Dialogue%20System/preface.html)

Quick overview of features
- Quick setup requiring only one script (the dialogue director) with ready made nodes for
  plug-and-play
- Uses Unity's [Graph Toolkit (GTK)](https://docs.unity3d.com/6000.5/Documentation/Manual/gtk/landing-graph-interface.html) for authoring
- Add choices and make branching dialogues with conditions
- Execute events or custom logic at any given point in the dialogue
- Integrate gameplay logic such as sequencing directly within the graph
- Realtime validation with quick actions to resolve issues
- Easily extend with custom nodes and processor using powerful templates
- Make use of subgraphs and asset subgraphs to organize and reuse dialogues

See [Getting Started]([getting-started.md](https://docs.clogic.dev/manuals/dev.clogic.dialogue/getting-started.html)) to get started with using the dialogue system.

See [Architecture]([architecture.md](https://docs.clogic.dev/manuals/dev.clogic.dialogue/architecture.html)) for more details on the architecture of the dialogue system.

## License & Usage

The CLogic Dialogue System is source-available, **not open-source** and free for **non-commercial** use.
For **commercial** use, you are required to purchase a license through the [Unity Asset Store](https://assetstore.unity.com/packages/package/6367127).
See the [License](https://github.com/CLogicDev/CLogic.Dialogue/blob/dev/LICENSE.md) for more info.