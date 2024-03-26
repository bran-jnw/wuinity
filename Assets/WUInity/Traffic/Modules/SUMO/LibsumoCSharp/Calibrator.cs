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

public class Calibrator : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal Calibrator(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(Calibrator obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~Calibrator() {
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
          libsumoPINVOKE.delete_Calibrator(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public static string getEdgeID(string calibratorID) {
    string ret = libsumoPINVOKE.Calibrator_getEdgeID(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static string getLaneID(string calibratorID) {
    string ret = libsumoPINVOKE.Calibrator_getLaneID(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static double getVehsPerHour(string calibratorID) {
    double ret = libsumoPINVOKE.Calibrator_getVehsPerHour(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static double getSpeed(string calibratorID) {
    double ret = libsumoPINVOKE.Calibrator_getSpeed(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static string getTypeID(string calibratorID) {
    string ret = libsumoPINVOKE.Calibrator_getTypeID(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static double getBegin(string calibratorID) {
    double ret = libsumoPINVOKE.Calibrator_getBegin(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static double getEnd(string calibratorID) {
    double ret = libsumoPINVOKE.Calibrator_getEnd(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static string getRouteID(string calibratorID) {
    string ret = libsumoPINVOKE.Calibrator_getRouteID(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static string getRouteProbeID(string calibratorID) {
    string ret = libsumoPINVOKE.Calibrator_getRouteProbeID(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringVector getVTypes(string calibratorID) {
    StringVector ret = new StringVector(libsumoPINVOKE.Calibrator_getVTypes(calibratorID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static int getPassed(string calibratorID) {
    int ret = libsumoPINVOKE.Calibrator_getPassed(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static int getInserted(string calibratorID) {
    int ret = libsumoPINVOKE.Calibrator_getInserted(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static int getRemoved(string calibratorID) {
    int ret = libsumoPINVOKE.Calibrator_getRemoved(calibratorID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringVector getIDList() {
    StringVector ret = new StringVector(libsumoPINVOKE.Calibrator_getIDList(), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static int getIDCount() {
    int ret = libsumoPINVOKE.Calibrator_getIDCount();
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static string getParameter(string objectID, string key) {
    string ret = libsumoPINVOKE.Calibrator_getParameter(objectID, key);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringStringPair getParameterWithKey(string objectID, string key) {
    StringStringPair ret = new StringStringPair(libsumoPINVOKE.Calibrator_getParameterWithKey(objectID, key), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void setParameter(string objectID, string key, string value) {
    libsumoPINVOKE.Calibrator_setParameter(objectID, key, value);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID, IntVector varIDs, double begin, double end, TraCIResults params_) {
    libsumoPINVOKE.Calibrator_subscribe__SWIG_0(objectID, IntVector.getCPtr(varIDs), begin, end, TraCIResults.getCPtr(params_));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID, IntVector varIDs, double begin, double end) {
    libsumoPINVOKE.Calibrator_subscribe__SWIG_1(objectID, IntVector.getCPtr(varIDs), begin, end);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID, IntVector varIDs, double begin) {
    libsumoPINVOKE.Calibrator_subscribe__SWIG_2(objectID, IntVector.getCPtr(varIDs), begin);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID, IntVector varIDs) {
    libsumoPINVOKE.Calibrator_subscribe__SWIG_3(objectID, IntVector.getCPtr(varIDs));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribe(string objectID) {
    libsumoPINVOKE.Calibrator_subscribe__SWIG_4(objectID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void unsubscribe(string objectID) {
    libsumoPINVOKE.Calibrator_unsubscribe(objectID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist, IntVector varIDs, double begin, double end, TraCIResults params_) {
    libsumoPINVOKE.Calibrator_subscribeContext__SWIG_0(objectID, domain, dist, IntVector.getCPtr(varIDs), begin, end, TraCIResults.getCPtr(params_));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist, IntVector varIDs, double begin, double end) {
    libsumoPINVOKE.Calibrator_subscribeContext__SWIG_1(objectID, domain, dist, IntVector.getCPtr(varIDs), begin, end);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist, IntVector varIDs, double begin) {
    libsumoPINVOKE.Calibrator_subscribeContext__SWIG_2(objectID, domain, dist, IntVector.getCPtr(varIDs), begin);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist, IntVector varIDs) {
    libsumoPINVOKE.Calibrator_subscribeContext__SWIG_3(objectID, domain, dist, IntVector.getCPtr(varIDs));
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeContext(string objectID, int domain, double dist) {
    libsumoPINVOKE.Calibrator_subscribeContext__SWIG_4(objectID, domain, dist);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void unsubscribeContext(string objectID, int domain, double dist) {
    libsumoPINVOKE.Calibrator_unsubscribeContext(objectID, domain, dist);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static SubscriptionResults getAllSubscriptionResults() {
    SubscriptionResults ret = new SubscriptionResults(libsumoPINVOKE.Calibrator_getAllSubscriptionResults(), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static TraCIResults getSubscriptionResults(string objectID) {
    TraCIResults ret = new TraCIResults(libsumoPINVOKE.Calibrator_getSubscriptionResults(objectID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static ContextSubscriptionResults getAllContextSubscriptionResults() {
    ContextSubscriptionResults ret = new ContextSubscriptionResults(libsumoPINVOKE.Calibrator_getAllContextSubscriptionResults(), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static SubscriptionResults getContextSubscriptionResults(string objectID) {
    SubscriptionResults ret = new SubscriptionResults(libsumoPINVOKE.Calibrator_getContextSubscriptionResults(objectID), true);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void subscribeParameterWithKey(string objectID, string key, double beginTime, double endTime) {
    libsumoPINVOKE.Calibrator_subscribeParameterWithKey__SWIG_0(objectID, key, beginTime, endTime);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeParameterWithKey(string objectID, string key, double beginTime) {
    libsumoPINVOKE.Calibrator_subscribeParameterWithKey__SWIG_1(objectID, key, beginTime);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void subscribeParameterWithKey(string objectID, string key) {
    libsumoPINVOKE.Calibrator_subscribeParameterWithKey__SWIG_2(objectID, key);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static int DOMAIN_ID {
    get {
      int ret = libsumoPINVOKE.Calibrator_DOMAIN_ID_get();
      if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public static void setFlow(string calibratorID, double begin, double end, double vehsPerHour, double speed, string typeID, string routeID, string departLane, string departSpeed) {
    libsumoPINVOKE.Calibrator_setFlow__SWIG_0(calibratorID, begin, end, vehsPerHour, speed, typeID, routeID, departLane, departSpeed);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void setFlow(string calibratorID, double begin, double end, double vehsPerHour, double speed, string typeID, string routeID, string departLane) {
    libsumoPINVOKE.Calibrator_setFlow__SWIG_1(calibratorID, begin, end, vehsPerHour, speed, typeID, routeID, departLane);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void setFlow(string calibratorID, double begin, double end, double vehsPerHour, double speed, string typeID, string routeID) {
    libsumoPINVOKE.Calibrator_setFlow__SWIG_2(calibratorID, begin, end, vehsPerHour, speed, typeID, routeID);
    if (libsumoPINVOKE.SWIGPendingException.Pending) throw libsumoPINVOKE.SWIGPendingException.Retrieve();
  }

}

}
