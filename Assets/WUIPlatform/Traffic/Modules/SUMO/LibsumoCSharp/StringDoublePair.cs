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

public class StringDoublePair : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal StringDoublePair(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(StringDoublePair obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~StringDoublePair() {
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
          libsumoPINVOKE.delete_StringDoublePair(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public StringDoublePair() : this(libsumoPINVOKE.new_StringDoublePair__SWIG_0(), true) {
  }

  public StringDoublePair(string first, double second) : this(libsumoPINVOKE.new_StringDoublePair__SWIG_1(first, second), true) {
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public StringDoublePair(StringDoublePair other) : this(libsumoPINVOKE.new_StringDoublePair__SWIG_2(StringDoublePair.getCPtr(other)), true) {
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public string first {
    set {
      libsumoPINVOKE.StringDoublePair_first_set(swigCPtr, value);
      if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      string ret = libsumoPINVOKE.StringDoublePair_first_get(swigCPtr);
      if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public double second {
    set {
      libsumoPINVOKE.StringDoublePair_second_set(swigCPtr, value);
    } 
    get {
      double ret = libsumoPINVOKE.StringDoublePair_second_get(swigCPtr);
      return ret;
    } 
  }

}

}
