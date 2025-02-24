using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace NewGameMode.Meme
{
    public class MemeButtonMovement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private bool isDragging = false; // 标记是否正在拖动按钮
        private Vector3 mouseOffset; // 鼠标和按钮的偏移量


        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                isDragging = true;
                // 计算鼠标位置和按钮位置的偏移量。eventdata是鼠标位置，
                mouseOffset = transform.position - new Vector3(eventData.position.x, eventData.position.y, 0);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 释放按钮时，停止拖动
                isDragging = false;
                // 确定新位置的索引
                int newIndex = GetNewIndex();
                // 更新子物体的顺序
                gameObject.transform.SetSiblingIndex(newIndex);
                LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.transform.parent.transform as RectTransform);

                //根据子物体顺序更新模因列表顺序

                Transform parentTransform = GetComponentInParent<GridLayoutGroup>().transform;
                List<MemeModel> newOrder = new List<MemeModel>();

                // 按sibling index顺序遍历子物体
                for (int i = 0; i < parentTransform.childCount; i++)
                {
                    Transform child = parentTransform.GetChild(i);
                    MemeButtonData model = child.GetComponent<MemeButtonData>();

                    if (model != null && model.memeModel != null)
                    {
                        newOrder.Add(model.memeModel);
                    }
                }

                // 更新模因的顺序并花钱
                MemeManager.instance.current_list.Clear();
                MemeManager.instance.current_list.AddRange(newOrder);
                WonderModel.instance.Pay(10);
            }  
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 当拖动时，更新按钮位置。组件移动按钮也跟着跑，是因为子对象的位置同样能影响父对象。
            if (isDragging)
            {
                Vector3 newPosition = new Vector3(eventData.position.x + mouseOffset.x, eventData.position.y + mouseOffset.y, 0);
                transform.position = newPosition;
            }
        }

        private int GetNewIndex()
        {
            int index = 1;
            // 实现逻辑来确定子物体的新索引
            // 这可能涉及到计算子物体在GridLayoutGroup中的位置和比较
            try
            {
                int x = 0;
                int y = 0;


                x = Convert.ToInt32(transform.localPosition.x) / 290;
                y = Convert.ToInt32(transform.localPosition.y) / -240;

                index = Math.Min(y * 4 + x, gameObject.transform.parent.childCount);
                if (index < 0)
                {
                    index = 0;
                }
                int count = y * 4 + x;

            }
            catch (Exception e)
            {
                Debug.Log(e.Message + Environment.NewLine + e.StackTrace);
            }
            return index; // 示例返回值
        }

    }
}
