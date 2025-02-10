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

public class Junction : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal Junction(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(Junction obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~Junction() {
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
          libsumoPINVOKE.delete_Junction(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public static TraCIPosition getPosition(string junctionID, bool includeZ) {
    TraCIPosition ret = new TraCIPosition(libsumoPINVOKE.Junction_getPosition__SWIG_0(junctionID, includeZ), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TraCIPosition getPosition(string junctionID) {
    TraCIPosition ret = new TraCIPosition(libsumoPINVOKE.Junction_getPosition__SWIG_1(junctionID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TraCIPositionVector getShape(string junctionID) {
    TraCIPositionVector ret = new TraCIPositionVector(libsumoPINVOKE.Junction_getShape(junctionID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringVector getIncomingEdges(string junctionID) {
    StringVector ret = new StringVector(libsumoPINVOKE.Junction_getIncomingEdges(junctionID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringVector getOutgoingEdges(string junctionID) {
    StringVector ret = new StringVector(libsumoPINVOKE.Junction_getOutgoingEdges(junctionID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringVector getIDList() {
    StringVector ret = new StringVector(libsumoPINVOKE.Junction_getIDList(), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static int getIDCount() {
    int ret = libsumoPINVOKE.Junction_getIDCount();
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static string getParameter(string objectID, string key) {
    string ret = libsumoPINVOKE.Junction_getParameter(objectID, key);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringStringPair getParameterWithKey(string objectID, string key) {
    StringStringPair ret = new StringStringPair(libsumoPINVOKE.Junction_getParameterWithKey(objectID, key), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void setParameter(string objectID, string key, string value) {
    libsumoPINVOKE.Junction_setParameter(objectID, key, value);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID, IntVector varIDs, double begin, double end, TraCIResults params_) {
    libsumoPINVOKE.Junction_subscribe__SWIG_0(objectID, IntVector.getCPtr(varIDs), begin, end, TraCIResults.getCPtr(params_));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID, IntVector varIDs, double begin, double end) {
    libsumoPINVOKE.Junction_subscribe__SWIG_1(objectID, IntVector.getCPtr(varIDs), begin, end);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID, IntVector varIDs, double begin) {
    libsumoPINVOKE.Junction_subscribe__SWIG_2(objectID, IntVector.getCPtr(varIDs), begin);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID, IntVector varIDs) {
    libsumoPINVOKE.Junction_subscribe__SWIG_3(objectID, IntVector.getCPtr(varIDs));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID) {
    libsumoPINVOKE.Junction_subscribe__SWIG_4(objectID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void unsubscribe(string objectID) {
    libsumoPINVOKE.Junction_unsubscribe(objectID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist, IntVector varIDs, double begin, double end, TraCIResults params_) {
    libsumoPINVOKE.Junction_subscribeContext__SWIG_0(objectID, domain, dist, IntVector.getCPtr(varIDs), begin, end, TraCIResults.getCPtr(params_));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist, IntVector varIDs, double begin, double end) {
    libsumoPINVOKE.Junction_subscribeContext__SWIG_1(objectID, domain, dist, IntVector.getCPtr(varIDs), begin, end);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist, IntVector varIDs, double begin) {
    libsumoPINVOKE.Junction_subscribeContext__SWIG_2(objectID, domain, dist, IntVector.getCPtr(varIDs), begin);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist, IntVector varIDs) {
    libsumoPINVOKE.Junction_subscribeContext__SWIG_3(objectID, domain, dist, IntVector.getCPtr(varIDs));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist) {
    libsumoPINVOKE.Junction_subscribeContext__SWIG_4(objectID, domain, dist);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void unsubscribeContext(string objectID, int domain, double dist) {
    libsumoPINVOKE.Junction_unsubscribeContext(objectID, domain, dist);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static SubscriptionResults getAllSubscriptionResults() {
    SubscriptionResults ret = new SubscriptionResults(libsumoPINVOKE.Junction_getAllSubscriptionResults(), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TraCIResults getSubscriptionResults(string objectID) {
    TraCIResults ret = new TraCIResults(libsumoPINVOKE.Junction_getSubscriptionResults(objectID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static ContextSubscriptionResults getAllContextSubscriptionResults() {
    ContextSubscriptionResults ret = new ContextSubscriptionResults(libsumoPINVOKE.Junction_getAllContextSubscriptionResults(), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SubscriptionResults getContextSubscriptionResults(string objectID) {
    SubscriptionResults ret = new SubscriptionResults(libsumoPINVOKE.Junction_getContextSubscriptionResults(objectID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void subscribeParameterWithKey(string objectID, string key, double beginTime, double endTime) {
    libsumoPINVOKE.Junction_subscribeParameterWithKey__SWIG_0(objectID, key, beginTime, endTime);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeParameterWithKey(string objectID, string key, double beginTime) {
    libsumoPINVOKE.Junction_subscribeParameterWithKey__SWIG_1(objectID, key, beginTime);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeParameterWithKey(string objectID, string key) {
    libsumoPINVOKE.Junction_subscribeParameterWithKey__SWIG_2(objectID, key);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static int DOMAIN_ID {
    get {
      int ret = libsumoPINVOKE.Junction_DOMAIN_ID_get();
      if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

}

}
