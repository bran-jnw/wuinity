//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace LIBSUMO {

public class TraCIStageVector : global::System.IDisposable, global::System.Collections.IEnumerable, global::System.Collections.Generic.IEnumerable<TraCIStage>
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal TraCIStageVector(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(TraCIStageVector obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~TraCIStageVector() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          libsumoPINVOKE.delete_TraCIStageVector(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public TraCIStageVector(global::System.Collections.IEnumerable c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (TraCIStage element in c) {
      this.Add(element);
    }
  }

  public TraCIStageVector(global::System.Collections.Generic.IEnumerable<TraCIStage> c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (TraCIStage element in c) {
      this.Add(element);
    }
  }

  public bool IsFixedSize {
    get {
      return false;
    }
  }

  public bool IsReadOnly {
    get {
      return false;
    }
  }

  public TraCIStage this[int index]  {
    get {
      return getitem(index);
    }
    set {
      setitem(index, value);
    }
  }

  public int Capacity {
    get {
      return (int)capacity();
    }
    set {
      if (value < size())
        throw new global::System.ArgumentOutOfRangeException("Capacity");
      reserve((uint)value);
    }
  }

  public int Count {
    get {
      return (int)size();
    }
  }

  public bool IsSynchronized {
    get {
      return false;
    }
  }

  public void CopyTo(TraCIStage[] array)
  {
    CopyTo(0, array, 0, this.Count);
  }

  public void CopyTo(TraCIStage[] array, int arrayIndex)
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

  public void CopyTo(int index, TraCIStage[] array, int arrayIndex, int count)
  {
    if (array == null)
      throw new global::System.ArgumentNullException("array");
    if (index < 0)
      throw new global::System.ArgumentOutOfRangeException("index", "Value is less than zero");
    if (arrayIndex < 0)
      throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
    if (count < 0)
      throw new global::System.ArgumentOutOfRangeException("count", "Value is less than zero");
    if (array.Rank > 1)
      throw new global::System.ArgumentException("Multi dimensional array.", "array");
    if (index+count > this.Count || arrayIndex+count > array.Length)
      throw new global::System.ArgumentException("Number of elements to copy is too large.");
    for (int i=0; i<count; i++)
      array.SetValue(getitemcopy(index+i), arrayIndex+i);
  }

  public TraCIStage[] ToArray() {
    TraCIStage[] array = new TraCIStage[this.Count];
    this.CopyTo(array);
    return array;
  }

  global::System.Collections.Generic.IEnumerator<TraCIStage> global::System.Collections.Generic.IEnumerable<TraCIStage>.GetEnumerator() {
    return new TraCIStageVectorEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new TraCIStageVectorEnumerator(this);
  }

  public TraCIStageVectorEnumerator GetEnumerator() {
    return new TraCIStageVectorEnumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class TraCIStageVectorEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<TraCIStage>
  {
    private TraCIStageVector collectionRef;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public TraCIStageVectorEnumerator(TraCIStageVector collection) {
      collectionRef = collection;
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public TraCIStage Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (TraCIStage)currentObject;
      }
    }

    // Type-unsafe IEnumerator.Current
    object global::System.Collections.IEnumerator.Current {
      get {
        return Current;
      }
    }

    public bool MoveNext() {
      int size = collectionRef.Count;
      bool moveOkay = (currentIndex+1 < size) && (size == currentSize);
      if (moveOkay) {
        currentIndex++;
        currentObject = collectionRef[currentIndex];
      } else {
        currentObject = null;
      }
      return moveOkay;
    }

    public void Reset() {
      currentIndex = -1;
      currentObject = null;
      if (collectionRef.Count != currentSize) {
        throw new global::System.InvalidOperationException("Collection modified.");
      }
    }

    public void Dispose() {
        currentIndex = -1;
        currentObject = null;
    }
  }

  public void Clear() {
    libsumoPINVOKE.TraCIStageVector_Clear(swigCPtr);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Add(TraCIStage x) {
    libsumoPINVOKE.TraCIStageVector_Add(swigCPtr, TraCIStage.getCPtr(x));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = libsumoPINVOKE.TraCIStageVector_size(swigCPtr);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private uint capacity() {
    uint ret = libsumoPINVOKE.TraCIStageVector_capacity(swigCPtr);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void reserve(uint n) {
    libsumoPINVOKE.TraCIStageVector_reserve(swigCPtr, n);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public TraCIStageVector() : this(libsumoPINVOKE.new_TraCIStageVector__SWIG_0(), true) {
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public TraCIStageVector(TraCIStageVector other) : this(libsumoPINVOKE.new_TraCIStageVector__SWIG_1(TraCIStageVector.getCPtr(other)), true) {
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public TraCIStageVector(int capacity) : this(libsumoPINVOKE.new_TraCIStageVector__SWIG_2(capacity), true) {
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  private TraCIStage getitemcopy(int index) {
    TraCIStage ret = new TraCIStage(libsumoPINVOKE.TraCIStageVector_getitemcopy(swigCPtr, index), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private TraCIStage getitem(int index) {
    TraCIStage ret = new TraCIStage(libsumoPINVOKE.TraCIStageVector_getitem(swigCPtr, index), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void setitem(int index, TraCIStage val) {
    libsumoPINVOKE.TraCIStageVector_setitem(swigCPtr, index, TraCIStage.getCPtr(val));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddRange(TraCIStageVector values) {
    libsumoPINVOKE.TraCIStageVector_AddRange(swigCPtr, TraCIStageVector.getCPtr(values));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public TraCIStageVector GetRange(int index, int count) {
    global::System.IntPtr cPtr = libsumoPINVOKE.TraCIStageVector_GetRange(swigCPtr, index, count);
    TraCIStageVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new TraCIStageVector(cPtr, true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Insert(int index, TraCIStage x) {
    libsumoPINVOKE.TraCIStageVector_Insert(swigCPtr, index, TraCIStage.getCPtr(x));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void InsertRange(int index, TraCIStageVector values) {
    libsumoPINVOKE.TraCIStageVector_InsertRange(swigCPtr, index, TraCIStageVector.getCPtr(values));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAt(int index) {
    libsumoPINVOKE.TraCIStageVector_RemoveAt(swigCPtr, index);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveRange(int index, int count) {
    libsumoPINVOKE.TraCIStageVector_RemoveRange(swigCPtr, index, count);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static TraCIStageVector Repeat(TraCIStage value, int count) {
    global::System.IntPtr cPtr = libsumoPINVOKE.TraCIStageVector_Repeat(TraCIStage.getCPtr(value), count);
    TraCIStageVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new TraCIStageVector(cPtr, true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Reverse() {
    libsumoPINVOKE.TraCIStageVector_Reverse__SWIG_0(swigCPtr);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Reverse(int index, int count) {
    libsumoPINVOKE.TraCIStageVector_Reverse__SWIG_1(swigCPtr, index, count);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetRange(int index, TraCIStageVector values) {
    libsumoPINVOKE.TraCIStageVector_SetRange(swigCPtr, index, TraCIStageVector.getCPtr(values));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

}

}
