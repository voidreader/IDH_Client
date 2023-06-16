using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDH.MyRoom
{
    public class TransformTile
    {
        public enum TileState
        {
            UseAble,
            Using,
            UseUnable,
        }

        public static Material TileColorNormal;
        public static Material TileColorRed;
        public static Material TileColorGreen;


        public TileState State { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public Transform Trans { get; private set; }
        public MeshRenderer Mesh_Renderer { get; private set; }
        public IPlaceAbleObject InUseMyRoomObject { get; set; }

        public TransformTile(int x, int y, Transform trans, MeshRenderer mRenderer)
        {
            X = x;
            Y = y;
            Trans = trans;
            Mesh_Renderer = mRenderer;
            SetState(TileState.UseAble);
        }

        public void SetState(TileState state)
        {
            switch (state)
            {
                case TileState.UseAble:
                    SetNormalColor();
                    break;
                case TileState.Using:
                    SetGreenColor();
                    break;
                case TileState.UseUnable:
                    SetRedColor();
                    break;
                default:
                    SetRedColor();
                    break;
            }

            State = state;
        }

        private void SetNormalColor() { Mesh_Renderer.material = TileColorNormal; Mesh_Renderer.enabled = true; }
        private void SetRedColor() { Mesh_Renderer.material = TileColorRed; Mesh_Renderer.enabled = true; }
        private void SetGreenColor() { Mesh_Renderer.material = TileColorGreen; Mesh_Renderer.enabled = true; }

        public static Vector2 Length(TransformTile first, TransformTile second)
        {
            return new Vector2(Mathf.Abs(first.X - second.X), Mathf.Abs(first.Y - second.Y));
        }
    }
}

