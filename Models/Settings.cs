using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWSManager.Models;

[JsonObject(MemberSerialization.OptOut)]
public partial class Settings : ReactiveObject
{
    /// <summary>
    /// Automatically check for updates every AutoCheckT hours
    /// </summary>
    [Reactive]
    private bool autoCheckUpdates = true;

    /// <summary>
    /// How often should update be checked automatically.
    /// </summary>
    [Reactive]
    private int autoCheckT = 6;
}
