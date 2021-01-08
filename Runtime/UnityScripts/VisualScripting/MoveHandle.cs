﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptingLanguage.VisualScripting
{
    public class MoveHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private Vector2 _startingPosition;
        private Dictionary<NodeComponent, Vector3> _nodesInitialPositions = new Dictionary<NodeComponent, Vector3>();
        public NodeComponent NodeComponent => GetComponentInParent<NodeComponent>();
        public Frame Frame => GetComponentInParent<Frame>();

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Middle) {
                return;
            }

            if (!Frame.Selected.Contains(this)) {
                Frame.UnselectAll();
                Frame.Select(this);
            }

            _startingPosition = eventData.position;
            _nodesInitialPositions.Clear();
            
            if (eventData.button == PointerEventData.InputButton.Right) {
                var newNodes = Frame.CopyNodes(Frame.Selected.Select(x => x.NodeComponent.Node));
                Frame.UnselectAll();
                Frame.Select(newNodes.Select(x => x.GetComponentInChildren<MoveHandle>()));
            }

            var allHandles = Frame.Selected.Select(x => x.GetComponentInChildren<MoveHandle>());
            foreach (var handle in allHandles) {
                _nodesInitialPositions[handle.NodeComponent] = handle.NodeComponent.transform.position;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Middle) {
                return;
            }
            Vector2 offset = eventData.position - _startingPosition;
            var nodeComponents = Frame.Selected.Select(x => x.NodeComponent);
            foreach (var node in nodeComponents) {
                node.transform.position = _nodesInitialPositions[node] + new Vector3(offset.x, offset.y, 0.0f);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Middle) {
                return;
            }

            if (eventData.pointerEnter == null) {
                return;
            }
            var trashCan = eventData.pointerEnter.GetComponent<TrashCan>();
            if (trashCan == null) {
                return;
            }
            var nodeComponents = Frame.Selected.Select(x => x.NodeComponent);
            foreach (var node in nodeComponents.ToList()) {
                node.DeInit();
                Destroy(node.gameObject);
            }
            Frame.UnselectAll();
        }
    }
}
