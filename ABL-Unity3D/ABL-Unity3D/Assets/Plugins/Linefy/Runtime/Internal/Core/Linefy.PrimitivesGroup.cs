using Linefy.Serialization;
using UnityEngine;

namespace Linefy {
    /// <summary>
    ///  Base class for Lines, Dots, Polylines
    /// </summary>
    public abstract class PrimitivesGroup : LinefyDrawcall {
        int id_widthMultiplier = Shader.PropertyToID("_WidthMultiplier");
        int id_persentOfScreenHeightMode = Shader.PropertyToID("_PersentOfScreenHeightMode");
 
        /// <summary>
        /// maximum count
        /// </summary>
        public virtual int maxCount {
            get {
                Debug.LogError("not implemeted");
                return 0;
            }
        }

        int prevCapacity;
        protected int capacity;
        protected int _count = -1;


        int _capacityChangeStep = 0;
        /// <summary>
        /// Determines how often the internal arrays capacity will change when count changes. 
        /// A lower value saves the GPU performance, but leads to a frequent allocation of memory when the count changing. 
        /// Set CapacityChangeStep = 1 in case of you do not plan to dynamically change the count. 
        /// </summary>
        public int capacityChangeStep {
            get {
                return _capacityChangeStep;
            }

            set {
                _capacityChangeStep = value;
            }
        }

        /// <summary>
        /// Number of elements
        /// </summary>
        public virtual int count {
            get {
                return _count;
            }

            set {
                int pCount = _count;
                int nCount = Mathf.Max(0, value);
                if (nCount > maxCount) {
                    Debug.LogWarningFormat("The count {0} is limited to the maximum value {1} for {2} ", nCount,  maxCount, GetType());
                    nCount = maxCount;
                }
                if (nCount != _count) {
                    _count = nCount;
                    if (capacityChangeStep <= 0) {
                        float blocks = Mathf.Max(1, _count / (float)16);
                        capacity =  Mathf.Max(capacity, Mathf.CeilToInt(blocks) * 64);
                    } else {
                        float blocks = Mathf.Max(1, _count / (float)capacityChangeStep);
                        capacity = Mathf.CeilToInt(blocks) * capacityChangeStep;
                    }
                    if (prevCapacity != capacity) {
                        SetCapacity(prevCapacity);
                        prevCapacity = capacity;
                    }
                    SetCount(pCount);
                }
            }
        }

        protected virtual void SetCapacity(int prevCapacity) {
            Debug.LogErrorFormat("SetCapacity() not implemented in {0}", GetType());
        }

        protected virtual void SetCount(int prevCount) {
            Debug.LogErrorFormat("SetCount() not implemented in {0}", GetType());
        }

        float _widthMultiplier = 1;
        /// <summary>
        /// Width factor. The used measuremnt units are defined by the <see cref="widthMode"/>
        /// </summary>
        public float widthMultiplier {
            get {
                return _widthMultiplier;
            }

            set {
                if (value != _widthMultiplier) {
                    _widthMultiplier = value;
                    material.SetFloat(id_widthMultiplier, _widthMultiplier);
                }
            }
        }

        WidthMode _widthMode = WidthMode.PixelsBillboard;
        /// <summary>
        /// Algorithm for calculating the width.
        /// </summary>
        public WidthMode widthMode {
            get {
                return _widthMode;
            }
            set {
                if (value != _widthMode) {
                    _widthMode = value;
                    ResetMaterial();
                    material.SetFloat(id_persentOfScreenHeightMode, (int)_widthMode == 2 ? 1 : 0);
                    material.SetFloat(id_widthMultiplier, _widthMultiplier);
                }
            }
        }

        protected override void OnAfterMaterialCreated() {
            base.OnAfterMaterialCreated();
            material.SetFloat(id_persentOfScreenHeightMode, (int)_widthMode == 2 ? 1 : 0);
            material.SetFloat(id_widthMultiplier, _widthMultiplier);
        }

        [System.Obsolete("SetVisualPropertyBlock is Obsolete , use LoadSerializationData instead")]
        public override void SetVisualPropertyBlock(VisualPropertiesBlock block) {
            base.SetVisualPropertyBlock(block);
            this.widthMode = block.widthMode;
        }

        /// <summary>
        /// Read and apply PrimitivesGroup data (deserialization)
        /// </summary>
        public void LoadSerializationData(SerializationData_PrimitivesGroup inputData) {
            LoadSerializationData((SerializationData_LinefyDrawcall)inputData);
            capacityChangeStep = inputData.capacityChangeStep;
            widthMultiplier = inputData.widthMultiplier;
            widthMode = inputData.widthMode;
        }

        /// <summary>
        /// Save PrimitivesGroup data (serialization)
        /// </summary>
        public void SaveSerializationData(SerializationData_PrimitivesGroup outputData) {
            SaveSerializationData((SerializationData_LinefyDrawcall)outputData);
            outputData.capacityChangeStep = capacityChangeStep;
            outputData.widthMultiplier = widthMultiplier;
            outputData.widthMode = widthMode;
        }
    }
}
