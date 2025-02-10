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

public class TraCISignalConstraintVector : global::System.IDisposable, global::System.Collections.IEnumerable, global::System.Collections.Generic.IEnumerable<TraCISignalConstraint>
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal TraCISignalConstraintVector(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(TraCISignalConstraintVector obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~TraCISignalConstraintVector() {
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
          libsumoPINVOKE.delete_TraCISignalConstraintVector(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public TraCISignalConstraintVector(global::System.Collections.IEnumerable c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (TraCISignalConstraint element in c) {
      this.Add(element);
    }
  }

  public TraCISignalConstraintVector(global::System.Collections.Generic.IEnumerable<TraCISignalConstraint> c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (TraCISignalConstraint element in c) {
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

  public TraCISignalConstraint this[int index]  {
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

  public void CopyTo(TraCISignalConstraint[] array)
  {
    CopyTo(0, array, 0, this.Count);
  }

  public void CopyTo(TraCISignalConstraint[] array, int arrayIndex)
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

  public void CopyTo(int index, TraCISignalConstraint[] array, int arrayIndex, int count)
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

  public TraCISignalConstraint[] ToArray() {
    TraCISignalConstraint[] array = new TraCISignalConstraint[this.Count];
    this.CopyTo(array);
    return array;
  }

  global::System.Collections.Generic.IEnumerator<TraCISignalConstraint> global::System.Collections.Generic.IEnumerable<TraCISignalConstraint>.GetEnumerator() {
    return new TraCISignalConstraintVectorEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new TraCISignalConstraintVectorEnumerator(this);
  }

  public TraCISignalConstraintVectorEnumerator GetEnumerator() {
    return new TraCISignalConstraintVectorEnumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class TraCISignalConstraintVectorEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<TraCISignalConstraint>
  {
    private TraCISignalConstraintVector collectionRef;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public TraCISignalConstraintVectorEnumerator(TraCISignalConstraintVector collection) {
      collectionRef = collection;
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public TraCISignalConstraint Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (TraCISignalConstraint)currentObject;
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
    libsumoPINVOKE.TraCISignalConstraintVector_Clear(swigCPtr);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Add(TraCISignalConstraint x) {
    libsumoPINVOKE.TraCISignalConstraintVector_Add(swigCPtr, TraCISignalConstraint.getCPtr(x));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = libsumoPINVOKE.TraCISignalConstraintVector_size(swigCPtr);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private uint capacity() {
    uint ret = libsumoPINVOKE.TraCISignalConstraintVector_capacity(swigCPtr);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void reserve(uint n) {
    libsumoPINVOKE.TraCISignalConstraintVector_reserve(swigCPtr, n);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public TraCISignalConstraintVector() : this(libsumoPINVOKE.new_TraCISignalConstraintVector__SWIG_0(), true) {
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public TraCISignalConstraintVector(TraCISignalConstraintVector other) : this(libsumoPINVOKE.new_TraCISignalConstraintVector__SWIG_1(TraCISignalConstraintVector.getCPtr(other)), true) {
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public TraCISignalConstraintVector(int capacity) : this(libsumoPINVOKE.new_TraCISignalConstraintVector__SWIG_2(capacity), true) {
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  private TraCISignalConstraint getitemcopy(int index) {
    TraCISignalConstraint ret = new TraCISignalConstraint(libsumoPINVOKE.TraCISignalConstraintVector_getitemcopy(swigCPtr, index), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private TraCISignalConstraint getitem(int index) {
    TraCISignalConstraint ret = new TraCISignalConstraint(libsumoPINVOKE.TraCISignalConstraintVector_getitem(swigCPtr, index), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void setitem(int index, TraCISignalConstraint val) {
    libsumoPINVOKE.TraCISignalConstraintVector_setitem(swigCPtr, index, TraCISignalConstraint.getCPtr(val));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddRange(TraCISignalConstraintVector values) {
    libsumoPINVOKE.TraCISignalConstraintVector_AddRange(swigCPtr, TraCISignalConstraintVector.getCPtr(values));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public TraCISignalConstraintVector GetRange(int index, int count) {
    global::System.IntPtr cPtr = libsumoPINVOKE.TraCISignalConstraintVector_GetRange(swigCPtr, index, count);
    TraCISignalConstraintVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new TraCISignalConstraintVector(cPtr, true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Insert(int index, TraCISignalConstraint x) {
    libsumoPINVOKE.TraCISignalConstraintVector_Insert(swigCPtr, index, TraCISignalConstraint.getCPtr(x));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void InsertRange(int index, TraCISignalConstraintVector values) {
    libsumoPINVOKE.TraCISignalConstraintVector_InsertRange(swigCPtr, index, TraCISignalConstraintVector.getCPtr(values));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAt(int index) {
    libsumoPINVOKE.TraCISignalConstraintVector_RemoveAt(swigCPtr, index);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveRange(int index, int count) {
    libsumoPINVOKE.TraCISignalConstraintVector_RemoveRange(swigCPtr, index, count);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static TraCISignalConstraintVector Repeat(TraCISignalConstraint value, int count) {
    global::System.IntPtr cPtr = libsumoPINVOKE.TraCISignalConstraintVector_Repeat(TraCISignalConstraint.getCPtr(value), count);
    TraCISignalConstraintVector ret = (cPtr == global::System.IntPtr.Zero) ? null : new TraCISignalConstraintVector(cPtr, true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Reverse() {
    libsumoPINVOKE.TraCISignalConstraintVector_Reverse__SWIG_0(swigCPtr);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void Reverse(int index, int count) {
    libsumoPINVOKE.TraCISignalConstraintVector_Reverse__SWIG_1(swigCPtr, index, count);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetRange(int index, TraCISignalConstraintVector values) {
    libsumoPINVOKE.TraCISignalConstraintVector_SetRange(swigCPtr, index, TraCISignalConstraintVector.getCPtr(values));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

}

}
