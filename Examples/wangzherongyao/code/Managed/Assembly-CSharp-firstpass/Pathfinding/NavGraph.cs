namespace Pathfinding
{
    using Pathfinding.Serialization;
    using Pathfinding.Serialization.JsonFx;
    using Pathfinding.Util;
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public abstract class NavGraph
    {
        public byte[] _sguid;
        [CompilerGenerated]
        private static GraphNodeDelegateCancelable <>f__am$cacheA;
        public AstarPath active;
        [JsonMember]
        public bool drawGizmos = true;
        public uint graphIndex;
        [JsonMember]
        public bool infoScreenOpen;
        [JsonMember]
        public uint initialPenalty;
        public Matrix4x4 inverseMatrix;
        [JsonMember]
        public Matrix4x4 matrix;
        [JsonMember]
        public string name;
        [JsonMember]
        public bool open;

        protected NavGraph()
        {
        }

        public virtual void Awake()
        {
        }

        public virtual int CountNodes()
        {
            <CountNodes>c__AnonStorey2B storeyb = new <CountNodes>c__AnonStorey2B {
                count = 0
            };
            GraphNodeDelegateCancelable del = new GraphNodeDelegateCancelable(storeyb.<>m__1E);
            this.GetNodes(del);
            return storeyb.count;
        }

        public virtual void CreateNodes(int number)
        {
            throw new NotSupportedException();
        }

        public virtual void DeserializeExtraInfo(GraphSerializationContext ctx)
        {
        }

        public virtual void DeserializeSettings(GraphSerializationContext ctx)
        {
            this.guid = new Pathfinding.Util.Guid(ctx.reader.ReadBytes(0x10));
            this.initialPenalty = ctx.reader.ReadUInt32();
            this.open = ctx.reader.ReadBoolean();
            this.name = ctx.reader.ReadString();
            this.drawGizmos = ctx.reader.ReadBoolean();
            this.infoScreenOpen = ctx.reader.ReadBoolean();
            for (int i = 0; i < 4; i++)
            {
                Vector4 zero = Vector4.zero;
                for (int j = 0; j < 4; j++)
                {
                    zero[j] = ctx.reader.ReadSingle();
                }
                this.matrix.SetRow(i, zero);
            }
        }

        protected virtual void Duplicate(NavGraph graph)
        {
            graph.active = this.active;
            graph.guid = this.guid;
            graph.initialPenalty = this.initialPenalty;
            graph.open = this.open;
            graph.graphIndex = this.graphIndex;
            graph.name = this.name;
            graph.drawGizmos = this.drawGizmos;
            graph.infoScreenOpen = this.infoScreenOpen;
            graph.matrix = this.matrix;
            graph.inverseMatrix = this.inverseMatrix;
        }

        public NNInfo GetNearest(Vector3 position)
        {
            return this.GetNearest(position, NNConstraint.None);
        }

        public NNInfo GetNearest(VInt3 position)
        {
            return this.GetNearest((Vector3) position, NNConstraint.None);
        }

        public NNInfo GetNearest(Vector3 position, NNConstraint constraint)
        {
            return this.GetNearest(position, constraint, null);
        }

        public NNInfo GetNearest(VInt3 position, NNConstraint constraint)
        {
            return this.GetNearest((Vector3) position, constraint, null);
        }

        public virtual NNInfo GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
        {
            <GetNearest>c__AnonStorey2D storeyd;
            storeyd = new <GetNearest>c__AnonStorey2D {
                position = position,
                constraint = constraint,
                maxDistSqr = !storeyd.constraint.constrainDistance ? float.PositiveInfinity : AstarPath.active.maxNearestNodeDistanceSqr,
                minDist = float.PositiveInfinity,
                minNode = null,
                minConstDist = float.PositiveInfinity,
                minConstNode = null
            };
            this.GetNodes(new GraphNodeDelegateCancelable(storeyd.<>m__20));
            NNInfo info = new NNInfo(storeyd.minNode) {
                constrainedNode = storeyd.minConstNode
            };
            if (storeyd.minConstNode != null)
            {
                info.constClampedPosition = (Vector3) storeyd.minConstNode.position;
                return info;
            }
            if (storeyd.minNode != null)
            {
                info.constrainedNode = storeyd.minNode;
                info.constClampedPosition = (Vector3) storeyd.minNode.position;
            }
            return info;
        }

        public virtual NNInfo GetNearestForce(Vector3 position, NNConstraint constraint)
        {
            return this.GetNearest(position, constraint);
        }

        public abstract void GetNodes(GraphNodeDelegateCancelable del);
        public static bool InSearchTree(GraphNode node, Path path)
        {
            if ((path != null) && (path.pathHandler != null))
            {
                return (path.pathHandler.GetPathNode(node).pathID == path.pathID);
            }
            return true;
        }

        public virtual Color NodeColor(GraphNode node, PathHandler data)
        {
            Color nodeConnection = AstarColor.NodeConnection;
            bool flag = false;
            if (node == null)
            {
                return AstarColor.NodeConnection;
            }
            GraphDebugMode debugMode = AstarPath.active.debugMode;
            switch (debugMode)
            {
                case GraphDebugMode.Penalty:
                    nodeConnection = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (node.Penalty - AstarPath.active.debugFloor) / (AstarPath.active.debugRoof - AstarPath.active.debugFloor));
                    flag = true;
                    break;

                case GraphDebugMode.Tags:
                    nodeConnection = AstarMath.IntToColor((int) node.Tag, 0.5f);
                    flag = true;
                    break;

                default:
                    if (debugMode == GraphDebugMode.Areas)
                    {
                        nodeConnection = AstarColor.GetAreaColor(node.Area);
                        flag = true;
                    }
                    break;
            }
            if (!flag)
            {
                if (data == null)
                {
                    return AstarColor.NodeConnection;
                }
                PathNode pathNode = data.GetPathNode(node);
                switch (AstarPath.active.debugMode)
                {
                    case GraphDebugMode.G:
                        nodeConnection = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (pathNode.G - AstarPath.active.debugFloor) / (AstarPath.active.debugRoof - AstarPath.active.debugFloor));
                        break;

                    case GraphDebugMode.H:
                        nodeConnection = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (pathNode.H - AstarPath.active.debugFloor) / (AstarPath.active.debugRoof - AstarPath.active.debugFloor));
                        break;

                    case GraphDebugMode.F:
                        nodeConnection = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (pathNode.F - AstarPath.active.debugFloor) / (AstarPath.active.debugRoof - AstarPath.active.debugFloor));
                        break;
                }
            }
            nodeConnection.a *= 0.5f;
            return nodeConnection;
        }

        public virtual void OnDestroy()
        {
            if (<>f__am$cacheA == null)
            {
                <>f__am$cacheA = delegate (GraphNode node) {
                    node.Destroy();
                    return true;
                };
            }
            this.GetNodes(<>f__am$cacheA);
        }

        public virtual void OnDrawGizmos(bool drawNodes)
        {
            <OnDrawGizmos>c__AnonStorey2E storeye = new <OnDrawGizmos>c__AnonStorey2E {
                <>f__this = this
            };
            if (drawNodes)
            {
                storeye.data = AstarPath.active.debugPathData;
                storeye.node = null;
                storeye.del = new GraphNodeDelegate(storeye.<>m__22);
                this.GetNodes(new GraphNodeDelegateCancelable(storeye.<>m__23));
            }
        }

        public virtual void PostDeserialization()
        {
        }

        public virtual void RelocateNodes(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
        {
            <RelocateNodes>c__AnonStorey2C storeyc = new <RelocateNodes>c__AnonStorey2C();
            Matrix4x4 inverse = oldMatrix.inverse;
            storeyc.m = inverse * newMatrix;
            this.GetNodes(new GraphNodeDelegateCancelable(storeyc.<>m__1F));
            this.SetMatrix(newMatrix);
        }

        public void SafeOnDestroy()
        {
            AstarPath.RegisterSafeUpdate(new OnVoidDelegate(this.OnDestroy));
        }

        [Obsolete("Please use AstarPath.active.Scan or if you really want this.ScanInternal which has the same functionality as this method had")]
        public void Scan()
        {
            throw new Exception("This method is deprecated. Please use AstarPath.active.Scan or if you really want this.ScanInternal which has the same functionality as this method had.");
        }

        public void ScanGraph()
        {
            if (AstarPath.OnPreScan != null)
            {
                AstarPath.OnPreScan(AstarPath.active);
            }
            if (AstarPath.OnGraphPreScan != null)
            {
                AstarPath.OnGraphPreScan(this);
            }
            this.ScanInternal();
            if (AstarPath.OnGraphPostScan != null)
            {
                AstarPath.OnGraphPostScan(this);
            }
            if (AstarPath.OnPostScan != null)
            {
                AstarPath.OnPostScan(AstarPath.active);
            }
        }

        public void ScanInternal()
        {
            this.ScanInternal(null);
        }

        public abstract void ScanInternal(OnScanStatus statusCallback);
        public virtual void SerializeExtraInfo(GraphSerializationContext ctx)
        {
        }

        public virtual void SerializeSettings(GraphSerializationContext ctx)
        {
            ctx.writer.Write(this.guid.ToByteArray());
            ctx.writer.Write(this.initialPenalty);
            ctx.writer.Write(this.open);
            ctx.writer.Write(this.name);
            ctx.writer.Write(this.drawGizmos);
            ctx.writer.Write(this.infoScreenOpen);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    ctx.writer.Write(this.matrix.GetRow(i)[j]);
                }
            }
        }

        public void SetMatrix(Matrix4x4 m)
        {
            this.matrix = m;
            this.inverseMatrix = m.inverse;
        }

        [JsonMember]
        public Pathfinding.Util.Guid guid
        {
            get
            {
                if ((this._sguid == null) || (this._sguid.Length != 0x10))
                {
                    this._sguid = Pathfinding.Util.Guid.NewGuid().ToByteArray();
                }
                return new Pathfinding.Util.Guid(this._sguid);
            }
            set
            {
                this._sguid = value.ToByteArray();
            }
        }

        [CompilerGenerated]
        private sealed class <CountNodes>c__AnonStorey2B
        {
            internal int count;

            internal bool <>m__1E(GraphNode node)
            {
                this.count++;
                return true;
            }
        }

        [CompilerGenerated]
        private sealed class <GetNearest>c__AnonStorey2D
        {
            internal NNConstraint constraint;
            internal float maxDistSqr;
            internal float minConstDist;
            internal GraphNode minConstNode;
            internal float minDist;
            internal GraphNode minNode;
            internal Vector3 position;

            internal bool <>m__20(GraphNode node)
            {
                Vector3 vector = this.position - ((Vector3) node.position);
                float sqrMagnitude = vector.sqrMagnitude;
                if (sqrMagnitude < this.minDist)
                {
                    this.minDist = sqrMagnitude;
                    this.minNode = node;
                }
                if (((sqrMagnitude < this.minConstDist) && (sqrMagnitude < this.maxDistSqr)) && this.constraint.Suitable(node))
                {
                    this.minConstDist = sqrMagnitude;
                    this.minConstNode = node;
                }
                return true;
            }
        }

        [CompilerGenerated]
        private sealed class <OnDrawGizmos>c__AnonStorey2E
        {
            internal NavGraph <>f__this;
            internal PathHandler data;
            internal GraphNodeDelegate del;
            internal GraphNode node;

            internal void <>m__22(GraphNode o)
            {
                Gizmos.DrawLine((Vector3) this.node.position, (Vector3) o.position);
            }

            internal bool <>m__23(GraphNode _node)
            {
                this.node = _node;
                Gizmos.color = this.<>f__this.NodeColor(this.node, AstarPath.active.debugPathData);
                if (!AstarPath.active.showSearchTree || NavGraph.InSearchTree(this.node, AstarPath.active.debugPath))
                {
                    PathNode node = (this.data == null) ? null : this.data.GetPathNode(this.node);
                    if ((AstarPath.active.showSearchTree && (node != null)) && (node.parent != null))
                    {
                        Gizmos.DrawLine((Vector3) this.node.position, (Vector3) node.parent.node.position);
                    }
                    else
                    {
                        this.node.GetConnections(this.del);
                    }
                }
                return true;
            }
        }

        [CompilerGenerated]
        private sealed class <RelocateNodes>c__AnonStorey2C
        {
            internal Matrix4x4 m;

            internal bool <>m__1F(GraphNode node)
            {
                node.position = (VInt3) this.m.MultiplyPoint((Vector3) node.position);
                return true;
            }
        }
    }
}

