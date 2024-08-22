using DunGen;
using DunGen.Graph;
using DunGen.Tags;
using HarmonyLib;
using LethalLevelLoader;
using System.Threading;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CentralConfig
{
    public class ShareDunGenData : NetworkBehaviour
    {
        public static ShareDunGenData Instance { get; private set; }

        public int dataVersion;

        public DungeonFlow lastSelectedDungeon = WaitForDungeonsToRegister.DefaultFacility.DungeonFlow;
        public LevelAmbienceLibrary lastLevelAmbience = LevelManager.CurrentExtendedLevel.SelectableLevel.levelAmbienceClips;
        public int lastDungeonType = 0;
        public int lastSeed = 0;
        public float lastDungeonSize = 0f;

        public ManualResetEvent dataReceivedEvent = new ManualResetEvent(false);

        private void Awake()
        {
            Instance = this;

            NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageName, OnMessageReceived);
        }
        [Tooltip("HostDG")]
        public string MessageName = "Host DunGen data";
        public void RequestData()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                var writer = new FastBufferWriter(0, Allocator.Temp);
                NetworkManager.CustomMessagingManager.SendNamedMessage(MessageName, NetworkManager.ServerClientId, writer);
            }
        }
        public void UpdateAndSendData()
        {
            using (var writer = new FastBufferWriter(1000000000, Allocator.Temp))
            {
                writer.WriteValueSafe(dataVersion);

                SerializeDungeonFlow(writer, lastSelectedDungeon);
                SerializeLevelAmbienceLibrary(writer, lastLevelAmbience);

                writer.WriteValueSafe(lastDungeonType);
                writer.WriteValueSafe(lastSeed);
                writer.WriteValueSafe(lastDungeonSize);

                NetworkManager.CustomMessagingManager.SendNamedMessage(MessageName, NetworkManager.ConnectedClientsIds, writer);
            }
        }
        private void OnMessageReceived(ulong clientId, FastBufferReader reader)
        {
            if (NetworkManager.Singleton.IsHost && clientId != NetworkManager.ServerClientId)
            {
                lastSelectedDungeon = CentralConfig.SelectedDungeon;
                lastLevelAmbience = CentralConfig.LevelAmbience;
                lastDungeonType = CentralConfig.DungeonType;
                lastSeed = CentralConfig.DunGenSeed;
                lastDungeonSize = CentralConfig.DungeonSize;
                dataVersion++;

                dataReceivedEvent.Set();

                UpdateAndSendData();
                CentralConfig.instance.mls.LogInfo("Host received message");
            }
            else if (!NetworkManager.Singleton.IsHost)
            {
                reader.ReadValueSafe(out int receivedVersion);
                if (receivedVersion > dataVersion)
                {
                    dataVersion = receivedVersion;
                    lastSelectedDungeon = DeserializeDungeonFlow(reader);
                    lastLevelAmbience = DeserializeLevelAmbienceLibrary(reader);

                    reader.ReadValueSafe(out int lastDungeonType);
                    reader.ReadValueSafe(out int lastSeed);
                    reader.ReadValueSafe(out float lastDungeonSize);

                    CentralConfig.SelectedDungeon = lastSelectedDungeon;
                    CentralConfig.LevelAmbience = lastLevelAmbience;
                    CentralConfig.DungeonType = lastDungeonType;
                    CentralConfig.DunGenSeed = lastSeed;
                    CentralConfig.DungeonSize = lastDungeonSize;

                    dataReceivedEvent.Set();
                    CentralConfig.instance.mls.LogInfo("Client received message");
                }
                else
                {
                    dataReceivedEvent.Reset();
                    RequestData();
                }
            }
        }
        private void Update()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (CentralConfig.SelectedDungeon != null && CentralConfig.LevelAmbience != null)
                {
                    if (CentralConfig.SelectedDungeon != lastSelectedDungeon || CentralConfig.LevelAmbience != lastLevelAmbience || CentralConfig.DungeonType != lastDungeonType || CentralConfig.DunGenSeed != lastSeed || CentralConfig.DungeonSize != lastDungeonSize)
                    {
                        UpdateAndSendData();

                        lastSelectedDungeon = CentralConfig.SelectedDungeon;
                        lastLevelAmbience = CentralConfig.LevelAmbience;
                        lastDungeonType = CentralConfig.DungeonType;
                        lastSeed = CentralConfig.DunGenSeed;
                        lastDungeonSize = CentralConfig.DungeonSize;

                        CentralConfig.instance.mls.LogInfo("Host has saved new DunGenData.");
                    }
                }
            }
        }
        private void SerializeDungeonFlow(FastBufferWriter writer, DungeonFlow data)
        {
            writer.WriteValueSafe(data.Length.Min);
            writer.WriteValueSafe(data.Length.Max);
            writer.WriteValueSafe((int)data.BranchMode);
            writer.WriteValueSafe(data.BranchCount.Min);
            writer.WriteValueSafe(data.BranchCount.Max);
            writer.WriteValueSafe(data.GlobalProps.Count);
            foreach (var prop in data.GlobalProps)
            {
                writer.WriteValueSafe(prop.ID);
                writer.WriteValueSafe(prop.Count.Min);
                writer.WriteValueSafe(prop.Count.Max);
            }
            writer.WriteValueSafe(data.KeyManager != null ? data.KeyManager.name : string.Empty);
            writer.WriteValueSafe(data.DoorwayConnectionChance);
            writer.WriteValueSafe(data.RestrictConnectionToSameSection);
            writer.WriteValueSafe((int)data.TileTagConnectionMode);
            writer.WriteValueSafe((int)data.BranchTagPruneMode);
            writer.WriteValueSafe(data.TileInjectionRules.Count);
            foreach (var rule in data.TileInjectionRules)
            {
                SerializeTileInjectionRule(writer, rule);
            }
            writer.WriteValueSafe(data.TileConnectionTags.Count);
            foreach (var tagPair in data.TileConnectionTags)
            {
                SerializeTag(writer, tagPair.TagA);
                SerializeTag(writer, tagPair.TagB);
            }
            writer.WriteValueSafe(data.BranchPruneTags.Count);
            foreach (var tag in data.BranchPruneTags)
            {
                SerializeTag(writer, tag);
            }
            writer.WriteValueSafe(data.Nodes.Count);
            foreach (var node in data.Nodes)
            {
                SerializeGraphNode(writer, node);
            }
            writer.WriteValueSafe(data.Lines.Count);
            foreach (var line in data.Lines)
            {
                SerializeGraphLine(writer, line);
            }
        }
        private void SerializeGraphLine(FastBufferWriter writer, GraphLine data)
        {
            writer.WriteValueSafe(data.Position);
            writer.WriteValueSafe(data.Length);
            writer.WriteValueSafe(data.DungeonArchetypes.Count);
            foreach (var archetype in data.DungeonArchetypes)
            {
                writer.WriteValueSafe(archetype != null ? archetype.name : string.Empty);
            }
            writer.WriteValueSafe(data.Keys.Count);
            foreach (var key in data.Keys)
            {
                SerializeKeyLockPlacement(writer, key);
            }
            writer.WriteValueSafe(data.Locks.Count);
            foreach (var lockPlacement in data.Locks)
            {
                SerializeKeyLockPlacement(writer, lockPlacement);
            }
        }
        private void SerializeGraphNode(FastBufferWriter writer, GraphNode data)
        {
            writer.WriteValueSafe(data.Position);
            writer.WriteValueSafe((int)data.NodeType);
            writer.WriteValueSafe(data.Label);
            writer.WriteValueSafe(data.TileSets.Count);
            foreach (var tileSet in data.TileSets)
            {
                writer.WriteValueSafe(tileSet != null ? tileSet.name : string.Empty);
            }
            writer.WriteValueSafe(data.Keys.Count);
            foreach (var key in data.Keys)
            {
                SerializeKeyLockPlacement(writer, key);
            }
            writer.WriteValueSafe(data.Locks.Count);
            foreach (var lockPlacement in data.Locks)
            {
                SerializeKeyLockPlacement(writer, lockPlacement);
            }
            SerializeNodeLockPlacement(writer, data.LockPlacement);
        }
        private void SerializeKeyLockPlacement(FastBufferWriter writer, KeyLockPlacement data)
        {
            writer.WriteValueSafe(data.ID);
            writer.WriteValueSafe(data.Range.Min);
            writer.WriteValueSafe(data.Range.Max);
        }
        private void SerializeNodeLockPlacement(FastBufferWriter writer, NodeLockPlacement data)
        {
            writer.WriteValueSafe((int)data);
        }
        private void SerializeTag(FastBufferWriter writer, Tag data)
        {
            writer.WriteValueSafe(data.ID);
        }
        private void SerializeTileInjectionRule(FastBufferWriter writer, TileInjectionRule data)
        {
            writer.WriteValueSafe(data.TileSet != null ? data.TileSet.name : string.Empty);
            writer.WriteValueSafe(data.NormalizedPathDepth.Min);
            writer.WriteValueSafe(data.NormalizedPathDepth.Max);
            writer.WriteValueSafe(data.NormalizedBranchDepth.Min);
            writer.WriteValueSafe(data.NormalizedBranchDepth.Max);
            writer.WriteValueSafe(data.CanAppearOnMainPath);
            writer.WriteValueSafe(data.CanAppearOnBranchPath);
            writer.WriteValueSafe(data.IsRequired);
            writer.WriteValueSafe(data.IsLocked);
            writer.WriteValueSafe(data.LockID);
        }
        private DungeonFlow DeserializeDungeonFlow(FastBufferReader reader)
        {
            DungeonFlow data = ScriptableObject.CreateInstance<DungeonFlow>();
            reader.ReadValueSafe(out int lengthMin);
            reader.ReadValueSafe(out int lengthMax);
            data.Length = new IntRange(lengthMin, lengthMax);
            reader.ReadValueSafe(out int branchMode);
            data.BranchMode = (BranchMode)branchMode;
            reader.ReadValueSafe(out int branchCountMin);
            reader.ReadValueSafe(out int branchCountMax);
            data.BranchCount = new IntRange(branchCountMin, branchCountMax);
            reader.ReadValueSafe(out int globalPropsCount);
            for (int i = 0; i < globalPropsCount; i++)
            {
                reader.ReadValueSafe(out int id);
                reader.ReadValueSafe(out int countMin);
                reader.ReadValueSafe(out int countMax);
                data.GlobalProps.Add(new DungeonFlow.GlobalPropSettings(id, new IntRange(countMin, countMax)));
            }
            reader.ReadValueSafe(out string keyManagerName);
            data.KeyManager = Resources.Load<KeyManager>(keyManagerName);
            reader.ReadValueSafe(out float doorwayConnectionChance);
            data.DoorwayConnectionChance = doorwayConnectionChance;
            reader.ReadValueSafe(out bool restrictConnectionToSameSection);
            data.RestrictConnectionToSameSection = restrictConnectionToSameSection;
            reader.ReadValueSafe(out int tileTagConnectionMode);
            data.TileTagConnectionMode = (DungeonFlow.TagConnectionMode)tileTagConnectionMode;
            reader.ReadValueSafe(out int branchTagPruneMode);
            data.BranchTagPruneMode = (DungeonFlow.BranchPruneMode)branchTagPruneMode;
            reader.ReadValueSafe(out int tileInjectionRulesCount);
            for (int i = 0; i < tileInjectionRulesCount; i++)
            {
                data.TileInjectionRules.Add(DeserializeTileInjectionRule(reader));
            }
            reader.ReadValueSafe(out int tileConnectionTagsCount);
            for (int i = 0; i < tileConnectionTagsCount; i++)
            {
                Tag tagA = DeserializeTag(reader);
                Tag tagB = DeserializeTag(reader);
                data.TileConnectionTags.Add(new TagPair(tagA, tagB));
            }
            reader.ReadValueSafe(out int branchPruneTagsCount);
            for (int i = 0; i < branchPruneTagsCount; i++)
            {
                data.BranchPruneTags.Add(DeserializeTag(reader));
            }
            reader.ReadValueSafe(out int nodesCount);
            for (int i = 0; i < nodesCount; i++)
            {
                data.Nodes.Add(DeserializeGraphNode(reader));
            }
            reader.ReadValueSafe(out int linesCount);
            for (int i = 0; i < linesCount; i++)
            {
                data.Lines.Add(DeserializeGraphLine(reader));
            }
            return data;
        }
        private GraphLine DeserializeGraphLine(FastBufferReader reader)
        {
            GraphLine data = new GraphLine(null);
            reader.ReadValueSafe(out data.Position);
            reader.ReadValueSafe(out data.Length);
            reader.ReadValueSafe(out int archetypesCount);
            for (int i = 0; i < archetypesCount; i++)
            {
                reader.ReadValueSafe(out string archetypeName);
                data.DungeonArchetypes.Add(Resources.Load<DungeonArchetype>(archetypeName));
            }
            reader.ReadValueSafe(out int keysCount);
            for (int i = 0; i < keysCount; i++)
            {
                data.Keys.Add(DeserializeKeyLockPlacement(reader));
            }
            reader.ReadValueSafe(out int locksCount);
            for (int i = 0; i < locksCount; i++)
            {
                data.Locks.Add(DeserializeKeyLockPlacement(reader));
            }
            return data;
        }
        private GraphNode DeserializeGraphNode(FastBufferReader reader)
        {
            GraphNode data = new GraphNode(null);
            reader.ReadValueSafe(out data.Position);
            reader.ReadValueSafe(out data.NodeType);
            reader.ReadValueSafe(out data.Label);
            reader.ReadValueSafe(out int tileSetsCount);
            for (int i = 0; i < tileSetsCount; i++)
            {
                reader.ReadValueSafe(out string tileSetName);
                data.TileSets.Add(Resources.Load<TileSet>(tileSetName));
            }
            reader.ReadValueSafe(out int keysCount);
            for (int i = 0; i < keysCount; i++)
            {
                data.Keys.Add(DeserializeKeyLockPlacement(reader));
            }
            reader.ReadValueSafe(out int locksCount);
            for (int i = 0; i < locksCount; i++)
            {
                data.Locks.Add(DeserializeKeyLockPlacement(reader));
            }
            data.LockPlacement = DeserializeNodeLockPlacement(reader);
            return data;
        }
        private KeyLockPlacement DeserializeKeyLockPlacement(FastBufferReader reader)
        {
            KeyLockPlacement data = new KeyLockPlacement();
            reader.ReadValueSafe(out data.ID);
            reader.ReadValueSafe(out int rangeMin);
            reader.ReadValueSafe(out int rangeMax);
            data.Range = new IntRange(rangeMin, rangeMax);
            return data;
        }
        private NodeLockPlacement DeserializeNodeLockPlacement(FastBufferReader reader)
        {
            reader.ReadValueSafe(out int lockPlacement);
            return (NodeLockPlacement)lockPlacement;
        }
        private Tag DeserializeTag(FastBufferReader reader)
        {
            reader.ReadValueSafe(out int id);
            return new Tag(id);
        }
        private TileInjectionRule DeserializeTileInjectionRule(FastBufferReader reader)
        {
            TileInjectionRule data = new TileInjectionRule();
            reader.ReadValueSafe(out string tileSetName);
            data.TileSet = Resources.Load<TileSet>(tileSetName);
            reader.ReadValueSafe(out float pathDepthMin);
            reader.ReadValueSafe(out float pathDepthMax);
            data.NormalizedPathDepth = new FloatRange(pathDepthMin, pathDepthMax);
            reader.ReadValueSafe(out float branchDepthMin);
            reader.ReadValueSafe(out float branchDepthMax);
            data.NormalizedBranchDepth = new FloatRange(branchDepthMin, branchDepthMax);
            reader.ReadValueSafe(out bool canAppearOnMainPath);
            data.CanAppearOnMainPath = canAppearOnMainPath;
            reader.ReadValueSafe(out bool canAppearOnBranchPath);
            data.CanAppearOnBranchPath = canAppearOnBranchPath;
            reader.ReadValueSafe(out bool isRequired);
            data.IsRequired = isRequired;
            reader.ReadValueSafe(out bool isLocked);
            data.IsLocked = isLocked;
            reader.ReadValueSafe(out int lockID);
            data.LockID = lockID;
            return data;
        }
        private void SerializeLevelAmbienceLibrary(FastBufferWriter writer, LevelAmbienceLibrary data)
        {
            SerializeAudioClipArray(writer, data.insanityMusicAudios);
            SerializeAudioClipArray(writer, data.insideAmbience);
            SerializeRandomAudioClipArray(writer, data.insideAmbienceInsanity);
            SerializeAudioClipArray(writer, data.shipAmbience);
            SerializeRandomAudioClipArray(writer, data.shipAmbienceInsanity);
            SerializeAudioClipArray(writer, data.outsideAmbience);
            SerializeRandomAudioClipArray(writer, data.outsideAmbienceInsanity);
        }
        private void SerializeAudioClipArray(FastBufferWriter writer, AudioClip[] clips)
        {
            writer.WriteValueSafe(clips.Length);
            foreach (var clip in clips)
            {
                string clipName = clip != null ? clip.name : string.Empty;
                writer.WriteValueSafe(clipName);
            }
        }
        private void SerializeRandomAudioClipArray(FastBufferWriter writer, RandomAudioClip[] clips)
        {
            writer.WriteValueSafe(clips.Length);
            foreach (var clip in clips)
            {
                string clipName = clip.audioClip != null ? clip.audioClip.name : string.Empty;
                writer.WriteValueSafe(clipName);
                writer.WriteValueSafe(clip.chance);
            }
        }
        private LevelAmbienceLibrary DeserializeLevelAmbienceLibrary(FastBufferReader reader)
        {
            LevelAmbienceLibrary data = ScriptableObject.CreateInstance<LevelAmbienceLibrary>();

            data.insanityMusicAudios = DeserializeAudioClipArray(reader);
            data.insideAmbience = DeserializeAudioClipArray(reader);
            data.insideAmbienceInsanity = DeserializeRandomAudioClipArray(reader);
            data.shipAmbience = DeserializeAudioClipArray(reader);
            data.shipAmbienceInsanity = DeserializeRandomAudioClipArray(reader);
            data.outsideAmbience = DeserializeAudioClipArray(reader);
            data.outsideAmbienceInsanity = DeserializeRandomAudioClipArray(reader);

            return data;
        }
        private AudioClip[] DeserializeAudioClipArray(FastBufferReader reader)
        {
            reader.ReadValueSafe(out int length);
            AudioClip[] clips = new AudioClip[length];
            for (int i = 0; i < length; i++)
            {
                reader.ReadValueSafe(out string clipName);
                clips[i] = Resources.Load<AudioClip>(clipName);
            }
            return clips;
        }
        private RandomAudioClip[] DeserializeRandomAudioClipArray(FastBufferReader reader)
        {
            reader.ReadValueSafe(out int length);
            RandomAudioClip[] clips = new RandomAudioClip[length];
            for (int i = 0; i < length; i++)
            {
                reader.ReadValueSafe(out string clipName);
                AudioClip audioClip = Resources.Load<AudioClip>(clipName);

                reader.ReadValueSafe(out int chance);
                clips[i] = new RandomAudioClip { audioClip = audioClip, chance = chance };
            }
            return clips;
        }
    }
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    public static class AnchorTheShare
    {
        static void Postfix(HangarShipDoor __instance)
        {
            if (__instance.gameObject.GetComponent<ShareDunGenData>() == null)
            {
                __instance.gameObject.AddComponent<ShareDunGenData>();
            }
        }
    }
}
