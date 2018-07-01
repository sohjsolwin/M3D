// Decompiled with JetBrains decompiler
// Type: M3D.Model.ModelData
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Model.Utils;
using MIConvexHull;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace M3D.Model
{
  public class ModelData
  {
    private List<M3D.Model.Utils.Vector3> Verticies;
    private HashSet<int> ConvexHullIndicies;
    private List<ModelData.EdgeIndex> EdgeIndecies;
    private List<ModelData.FaceIndex> FaceIndecies;
    private List<ModelData.VertexIndex> VertexIndecies;

    private ModelData(List<ModelData.FaceIndex> newFaceList, List<M3D.Model.Utils.Vector3> newVerticesList, List<ModelData.VertexIndex> newVertexIndecies)
    {
      this.FaceIndecies = newFaceList;
      this.Verticies = newVerticesList;
      this.VertexIndecies = newVertexIndecies;
    }

    public M3D.Model.Utils.Vector3 this[int index]
    {
      get
      {
        return new M3D.Model.Utils.Vector3(this.Verticies[index]);
      }
    }

    public M3D.Model.Utils.Vector3 Min { private set; get; }

    public M3D.Model.Utils.Vector3 Max { private set; get; }

    public M3D.Model.Utils.Vector3 Ext
    {
      get
      {
        return this.Max - this.Min;
      }
    }

    public M3D.Model.Utils.Vector3 Center
    {
      get
      {
        return (this.Max - this.Min) / 2f + this.Min;
      }
    }

    private void ProcessMinMaxBounds()
    {
      float maxValue1 = float.MaxValue;
      float maxValue2 = float.MaxValue;
      float maxValue3 = float.MaxValue;
      float minValue1 = float.MinValue;
      float minValue2 = float.MinValue;
      float minValue3 = float.MinValue;
      List<M3D.Model.Utils.Vector3> convexHullVertices = this.getAllConvexHullVertices();
      for (int index = 0; index < convexHullVertices.Count; ++index)
      {
        this.MinMaxBounds_Helper(ref maxValue1, ref minValue1, convexHullVertices[index].x);
        this.MinMaxBounds_Helper(ref maxValue2, ref minValue2, convexHullVertices[index].y);
        this.MinMaxBounds_Helper(ref maxValue3, ref minValue3, convexHullVertices[index].z);
      }
      this.Min = new M3D.Model.Utils.Vector3(maxValue1, maxValue2, maxValue3);
      this.Max = new M3D.Model.Utils.Vector3(minValue1, minValue2, minValue3);
    }

    public void GetMinMaxWithTransform(Matrix4 matrix, out M3D.Model.Utils.Vector3 min, out M3D.Model.Utils.Vector3 max)
    {
      float maxValue1 = float.MaxValue;
      float maxValue2 = float.MaxValue;
      float maxValue3 = float.MaxValue;
      float minValue1 = float.MinValue;
      float minValue2 = float.MinValue;
      float minValue3 = float.MinValue;
      List<M3D.Model.Utils.Vector3> convexHullVertices = this.getAllConvexHullVertices();
      for (int index = 0; index < convexHullVertices.Count; ++index)
      {
        M3D.Model.Utils.Vector3 vector3 = M3D.Model.Utils.Vector3.MatrixProduct(matrix, convexHullVertices[index]);
        this.MinMaxBounds_Helper(ref maxValue1, ref minValue1, vector3.x);
        this.MinMaxBounds_Helper(ref maxValue2, ref minValue2, vector3.y);
        this.MinMaxBounds_Helper(ref maxValue3, ref minValue3, vector3.z);
      }
      min = new M3D.Model.Utils.Vector3(maxValue1, maxValue2, maxValue3);
      max = new M3D.Model.Utils.Vector3(minValue1, minValue2, minValue3);
    }

    private void MinMaxBounds_Helper(ref float min, ref float max, float var)
    {
      if ((double) min > (double) var)
        min = var;
      if ((double) max >= (double) var)
        return;
      max = var;
    }

    public List<ModelData.Vertex> GetAllVertexs()
    {
      List<ModelData.Vertex> vertexList = new List<ModelData.Vertex>(this.Verticies.Count<M3D.Model.Utils.Vector3>());
      for (int index = 0; index < this.Verticies.Count; ++index)
        vertexList.Add(this.getVertex(index));
      return vertexList;
    }

    public List<ModelData.Face> GetAllFaces()
    {
      List<ModelData.Face> faceList = new List<ModelData.Face>(this.FaceIndecies.Count);
      for (int index = 0; index < this.FaceIndecies.Count; ++index)
        faceList.Add(this.getFace(index));
      return faceList;
    }

    public ModelData.Vertex getVertex(int index)
    {
      return new ModelData.Vertex(this, index);
    }

    public ModelData.Face getFace(int index)
    {
      return new ModelData.Face(this, index);
    }

    public int getVertexCount()
    {
      return this.Verticies.Count;
    }

    public int getFaceCount()
    {
      return this.FaceIndecies.Count;
    }

    public void Translate(M3D.Model.Utils.Vector3 translate)
    {
      for (int index = 0; index < this.Verticies.Count; ++index)
        this.Verticies[index] += translate;
      this.Min += translate;
      this.Max += translate;
    }

    public void Scale(M3D.Model.Utils.Vector3 scale)
    {
      for (int index = 0; index < this.Verticies.Count; ++index)
        this.Verticies[index] *= scale;
      this.Min *= scale;
      this.Max *= scale;
    }

    public void Transform(Matrix4 transform)
    {
      for (int index = 0; index < this.Verticies.Count; ++index)
        this.Verticies[index].MatrixProduct(transform);
      this.ProcessMinMaxBounds();
    }

    private void Clear()
    {
      this.FaceIndecies.Clear();
      this.VertexIndecies.Clear();
      this.Verticies.Clear();
    }

    private void InitalizeConvexHull(List<ModelData.Vertex> points)
    {
      ConvexHullIndicies = new HashSet<int>(collection: ConvexHull.Create(points.ConvertAll(x => (IVertex)x), null).Points.ToList().ConvertAll(x => ((Vertex)x).Index));
    }

    public List<M3D.Model.Utils.Vector3> getAllConvexHullVertices()
    {
      List<M3D.Model.Utils.Vector3> vector3List = new List<M3D.Model.Utils.Vector3>(this.ConvexHullIndicies.Count);
      foreach (int convexHullIndicy in this.ConvexHullIndicies)
        vector3List.Add(this.Verticies[convexHullIndicy]);
      return vector3List;
    }

    public List<M3D.Model.Utils.Vector3> CalculateHullPointsUsingTransformMatrix(Matrix4 matrix)
    {
      List<M3D.Model.Utils.Vector3> vector3List = new List<M3D.Model.Utils.Vector3>(this.ConvexHullIndicies.Count);
      foreach (int convexHullIndicy in this.ConvexHullIndicies)
      {
        M3D.Model.Utils.Vector3 vector3 = new M3D.Model.Utils.Vector3(this.Verticies[convexHullIndicy]);
        vector3.MatrixProduct(matrix);
        vector3List.Add(vector3);
      }
      return vector3List;
    }

    public bool isConvexHullPoint(int VertexIndex)
    {
      return this.ConvexHullIndicies.Contains(VertexIndex);
    }

    internal static ModelData Create(LinkedList<M3D.Model.Utils.Vector3> verticies, LinkedList<int[]> triangleIndecies = null, ProgressHelper.PercentageDelagate percentageDeligate = null)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      ProgressHelper progressHelper = new ProgressHelper(percentageDeligate, 3);
      if (triangleIndecies != null)
        ModelData.GenerateOrderedList(ref verticies, triangleIndecies);
      if (verticies.Count % 3 != 0)
        return (ModelData) null;
      progressHelper.SetStage(verticies.Count<M3D.Model.Utils.Vector3>());
      List<M3D.Model.Utils.Vector3> newVerticesList;
      List<ModelData.FaceIndex> newFaceList;
      if (!ModelData.HashVertexesAndFaces_Helper(verticies, ref progressHelper, out newVerticesList, out newFaceList))
        return (ModelData) null;
      progressHelper.SetStage(newFaceList.Count<ModelData.FaceIndex>());
      List<ModelData.VertexIndex> faceLinkHelper = ModelData.GenerateFaceLink_Helper(newVerticesList, newFaceList, progressHelper);
      ModelData modelData = new ModelData(newFaceList, newVerticesList, faceLinkHelper);
      modelData.InitalizeConvexHull(modelData.GetAllVertexs());
      modelData.ProcessMinMaxBounds();
      stopwatch.Stop();
      if (percentageDeligate == null)
        return modelData;
      percentageDeligate(100);
      return modelData;
    }

    private static void GenerateOrderedList(ref LinkedList<M3D.Model.Utils.Vector3> verticesList, LinkedList<int[]> faceIndicesList)
    {
      M3D.Model.Utils.Vector3[] vector3Array = new M3D.Model.Utils.Vector3[verticesList.Count];
      LinkedListNode<M3D.Model.Utils.Vector3> linkedListNode1 = verticesList.First;
      for (int index = 0; index < verticesList.Count; ++index)
      {
        vector3Array[index] = linkedListNode1.Value;
        linkedListNode1 = linkedListNode1.Next;
      }
      verticesList.Clear();
      LinkedListNode<int[]> linkedListNode2 = faceIndicesList.First;
      while (linkedListNode2 != null)
      {
        int[] numArray = linkedListNode2.Value;
        linkedListNode2 = linkedListNode2.Next;
        switch (numArray.Length)
        {
          case 0:
            continue;
          case 3:
            foreach (int index in numArray)
              verticesList.AddLast(vector3Array[index]);
            continue;
          case 4:
            for (int index = 0; index < 3; ++index)
              verticesList.AddLast(vector3Array[index]);
            for (int index = 0; index < 3; ++index)
              verticesList.AddLast(vector3Array[index + 1]);
            continue;
          default:
            throw new Exception(string.Format("ExpandVertices: was give a face with {0} indicies", (object) numArray.Length));
        }
      }
    }

    private static bool HashVertexesAndFaces_Helper(LinkedList<M3D.Model.Utils.Vector3> verticies, ref ProgressHelper progressHelper, out List<M3D.Model.Utils.Vector3> newVerticesList, out List<ModelData.FaceIndex> newFaceList)
    {
      newVerticesList = (List<M3D.Model.Utils.Vector3>) null;
      newFaceList = (List<ModelData.FaceIndex>) null;
      int num1 = 0;
      int num2 = 0;
      Dictionary<M3D.Model.Utils.Vector3, int> source = new Dictionary<M3D.Model.Utils.Vector3, int>();
      HashSet<ModelData.FaceIndex> faceIndexSet = new HashSet<ModelData.FaceIndex>();
      bool flag = ModelData.verticiesFlipped(verticies);
      while (verticies.First != null)
      {
        M3D.Model.Utils.Vector3 vector3_1 = verticies.First.Value;
        ModelData.AssertIfNANOrNULL(vector3_1);
        verticies.RemoveFirst();
        M3D.Model.Utils.Vector3 vector3_2 = verticies.First.Value;
        ModelData.AssertIfNANOrNULL(vector3_2);
        verticies.RemoveFirst();
        M3D.Model.Utils.Vector3 vector3_3 = verticies.First.Value;
        ModelData.AssertIfNANOrNULL(vector3_3);
        verticies.RemoveFirst();
        if (flag)
        {
          M3D.Model.Utils.Vector3 vector3_4 = vector3_1;
          vector3_1 = vector3_3;
          vector3_3 = vector3_4;
        }
        M3D.Model.Utils.Vector3 normal = ModelData.CalcNormal(vector3_1, vector3_2, vector3_3);
        if (!double.IsNaN((double) normal.x) && !double.IsNaN((double) normal.y) && !double.IsNaN((double) normal.z))
        {
          int[] faceIndicies = new int[3];
          M3D.Model.Utils.Vector3[] vector3Array = new M3D.Model.Utils.Vector3[3]
          {
            vector3_1,
            vector3_2,
            vector3_3
          };
          for (int index = 0; index < 3; ++index)
          {
            M3D.Model.Utils.Vector3 key = vector3Array[index];
            int num3;
            if (source.ContainsKey(key))
            {
              num3 = source[key];
            }
            else
            {
              source.Add(key, num1);
              num3 = num1++;
            }
            faceIndicies[index] = num3;
          }
          for (int index1 = 0; index1 < 3; ++index1)
          {
            for (int index2 = index1 + 1; index2 < 3; ++index2)
            {
              if (faceIndicies[index1] == faceIndicies[index2])
                return false;
            }
          }
          ModelData.FaceIndex faceIndex = new ModelData.FaceIndex(faceIndicies, normal);
          if (!faceIndexSet.Contains(faceIndex))
            faceIndexSet.Add(faceIndex);
          progressHelper.Process(num2 += 3);
        }
      }
      IEnumerable<M3D.Model.Utils.Vector3> collection = source.OrderBy<KeyValuePair<M3D.Model.Utils.Vector3, int>, int>((Func<KeyValuePair<M3D.Model.Utils.Vector3, int>, int>) (pair => pair.Value)).Select<KeyValuePair<M3D.Model.Utils.Vector3, int>, M3D.Model.Utils.Vector3>((Func<KeyValuePair<M3D.Model.Utils.Vector3, int>, M3D.Model.Utils.Vector3>) (pair => pair.Key));
      newVerticesList = new List<M3D.Model.Utils.Vector3>(collection);
      source.Clear();
      newFaceList = new List<ModelData.FaceIndex>((IEnumerable<ModelData.FaceIndex>) faceIndexSet);
      faceIndexSet.Clear();
      return true;
    }

    private static void AssertIfNANOrNULL(M3D.Model.Utils.Vector3 p1)
    {
      if (p1 == (M3D.Model.Utils.Vector3) null)
        throw new NullReferenceException("Null point in loading");
      if (double.IsNaN((double) p1.x) || double.IsNaN((double) p1.y) || double.IsNaN((double) p1.z))
        throw new Exception("Point contains a NAN");
    }

    private static List<ModelData.VertexIndex> GenerateFaceLink_Helper(List<M3D.Model.Utils.Vector3> newVerticesList, List<ModelData.FaceIndex> newFaceList, ProgressHelper progressHelper = null)
    {
      List<ModelData.VertexIndex> vertexIndexList = new List<ModelData.VertexIndex>(newFaceList.Count);
      foreach (M3D.Model.Utils.Vector3 newVertices in newVerticesList)
        vertexIndexList.Add(new ModelData.VertexIndex(new List<int>()));
      for (int index = 0; index < newFaceList.Count; ++index)
      {
        ModelData.FaceIndex newFace = newFaceList[index];
        vertexIndexList[newFace.P1].Faces.Add(index);
        vertexIndexList[newFace.P2].Faces.Add(index);
        vertexIndexList[newFace.P3].Faces.Add(index);
        progressHelper?.Process(index);
      }
      return vertexIndexList;
    }

    private static List<ModelData.EdgeIndex> GenerateEndgeList(List<ModelData.FaceIndex> faceList, ref ProgressHelper progressHelper)
    {
      Dictionary<ModelData.EdgeIndex, ModelData.EdgeIndex> hashCollection = new Dictionary<ModelData.EdgeIndex, ModelData.EdgeIndex>();
      for (int index = 0; index < faceList.Count; ++index)
      {
        ModelData.FaceIndex face = faceList[index];
        ModelData.AppendEdgeToHash_Helper(ref hashCollection, index, face.P1, face.P2);
        ModelData.AppendEdgeToHash_Helper(ref hashCollection, index, face.P2, face.P3);
        ModelData.AppendEdgeToHash_Helper(ref hashCollection, index, face.P1, face.P3);
        progressHelper.Process(index);
      }
      return hashCollection.Values.ToList<ModelData.EdgeIndex>();
    }

    private static void AppendEdgeToHash_Helper(ref Dictionary<ModelData.EdgeIndex, ModelData.EdgeIndex> hashCollection, int faceId, int P1_ID, int P2_ID)
    {
      ModelData.EdgeIndex key = new ModelData.EdgeIndex(P1_ID, P2_ID);
      if (!hashCollection.ContainsKey(key))
      {
        key.Faces = new List<int>() { faceId };
        hashCollection.Add(key, key);
      }
      else
      {
        ModelData.EdgeIndex edgeIndex = hashCollection[key];
        if (edgeIndex.Faces.Contains(faceId))
          return;
        edgeIndex.Faces.Add(faceId);
      }
    }

    private static bool CommonWindingTest_Helper(int[] face1, int[] face2, int[] common)
    {
      int num1 = 0;
      int num2 = 0;
      for (int index = 0; index < 3; ++index)
      {
        if (num1 == 0)
        {
          if (face1[index] == common[0])
            num1 = -1;
          else if (face1[index] == common[1])
            num1 = 1;
        }
        if (num2 == 0)
        {
          if (face2[index] == common[0])
            num2 = -1;
          else if (face2[index] == common[1])
            num2 = 1;
        }
      }
      return num1 + num2 == 0;
    }

    private static bool verticiesFlipped(List<ModelData.FaceIndex> faces, List<M3D.Model.Utils.Vector3> vertices)
    {
      return ModelData.GenerateSignedVolume(faces, vertices) < 0.0;
    }

    private static double GenerateSignedVolume(List<ModelData.FaceIndex> faces, List<M3D.Model.Utils.Vector3> vertices)
    {
      double num1 = 0.0;
      for (int index = 0; index < faces.Count; ++index)
      {
        M3D.Model.Utils.Vector3 vertex1 = vertices[faces[index].P1];
        M3D.Model.Utils.Vector3 vertex2 = vertices[faces[index].P2];
        M3D.Model.Utils.Vector3 vertex3 = vertices[faces[index].P3];
        double num2 = (double) vertex1.x * (double) vertex2.y * (double) vertex3.z + (double) vertex1.y * (double) vertex2.z * (double) vertex3.x + (double) vertex1.z * (double) vertex2.x * (double) vertex3.y - (double) vertex1.z * (double) vertex2.y * (double) vertex3.x - (double) vertex1.y * (double) vertex2.x * (double) vertex3.z - (double) vertex1.x * (double) vertex2.z * (double) vertex3.y;
        num1 += num2 / 6.0;
      }
      return num1;
    }

    private static bool verticiesFlipped(LinkedList<M3D.Model.Utils.Vector3> linkedVertex)
    {
      double num1 = 0.0;
      LinkedListNode<M3D.Model.Utils.Vector3> linkedListNode = linkedVertex.First;
      while (linkedListNode != null)
      {
        M3D.Model.Utils.Vector3 vector3_1 = linkedListNode.Value;
        LinkedListNode<M3D.Model.Utils.Vector3> next1 = linkedListNode.Next;
        M3D.Model.Utils.Vector3 vector3_2 = next1.Value;
        LinkedListNode<M3D.Model.Utils.Vector3> next2 = next1.Next;
        M3D.Model.Utils.Vector3 vector3_3 = next2.Value;
        linkedListNode = next2.Next;
        double num2 = (double) vector3_1.x * (double) vector3_2.y * (double) vector3_3.z + (double) vector3_1.y * (double) vector3_2.z * (double) vector3_3.x + (double) vector3_1.z * (double) vector3_2.x * (double) vector3_3.y - (double) vector3_1.z * (double) vector3_2.y * (double) vector3_3.x - (double) vector3_1.y * (double) vector3_2.x * (double) vector3_3.z - (double) vector3_1.x * (double) vector3_2.z * (double) vector3_3.y;
        num1 += num2 / 6.0;
      }
      return num1 < 0.0;
    }

    public static M3D.Model.Utils.Vector3 CalcNormal(M3D.Model.Utils.Vector3 _a, M3D.Model.Utils.Vector3 _b, M3D.Model.Utils.Vector3 _c)
    {
      float num1 = _b.x - _a.x;
      float num2 = _b.y - _a.y;
      float num3 = _b.z - _a.z;
      float num4 = _c.x - _a.x;
      float num5 = _c.y - _a.y;
      float num6 = _c.z - _a.z;
      double num7 = (double) num2 * (double) num6 - (double) num3 * (double) num5;
      float num8 = (float) ((double) num3 * (double) num4 - (double) num1 * (double) num6);
      float num9 = (float) ((double) num1 * (double) num5 - (double) num2 * (double) num4);
      float num10 = (float) Math.Sqrt(num7 * num7 + (double) num8 * (double) num8 + (double) num9 * (double) num9);
      return new M3D.Model.Utils.Vector3((float) num7 / num10, num8 / num10, num9 / num10);
    }

    public static ModelData Stitch(List<ModelTransform> modelTransforms)
    {
      int capacity1 = 0;
      int capacity2 = 0;
      foreach (ModelTransform modelTransform in modelTransforms)
      {
        capacity1 += modelTransform.data.getFaceCount();
        capacity2 += modelTransform.data.getVertexCount();
      }
      List<ModelData.FaceIndex> newFaceList = new List<ModelData.FaceIndex>(capacity1);
      List<M3D.Model.Utils.Vector3> newVerticesList = new List<M3D.Model.Utils.Vector3>(capacity2);
      int num1 = 0;
      int num2 = 0;
      foreach (ModelTransform modelTransform in modelTransforms)
      {
        int faceCount = modelTransform.data.getFaceCount();
        int vertexCount = modelTransform.data.getVertexCount();
        for (int index = 0; index < faceCount; ++index)
        {
          ModelData.Face face = modelTransform.data.getFace(index);
          newFaceList.Add(new ModelData.FaceIndex(face.index1 + num2, face.index2 + num2, face.index3 + num2, face.Normal));
        }
        for (int index = 0; index < vertexCount; ++index)
        {
          M3D.Model.Utils.Vector3 vector3 = M3D.Model.Utils.Vector3.MatrixProduct(modelTransform.transformMatrix, modelTransform.data[index]);
          newVerticesList.Add(vector3);
        }
        num1 += faceCount;
        num2 += vertexCount;
      }
      List<ModelData.VertexIndex> faceLinkHelper = ModelData.GenerateFaceLink_Helper(newVerticesList, newFaceList, (ProgressHelper) null);
      return new ModelData(newFaceList, newVerticesList, faceLinkHelper);
    }

    private class Edge : IEquatable<ModelData.Edge>
    {
      private int index;
      private ModelData model;

      public bool Equals(ModelData.Edge other)
      {
        return this.index == other.index;
      }

      private List<ModelData.Face> GetFaces
      {
        get
        {
          List<ModelData.Face> faceList = new List<ModelData.Face>();
          foreach (int face in this.model.EdgeIndecies[this.index].Faces)
            faceList.Add(new ModelData.Face(this.model, face));
          return faceList;
        }
      }

      private ModelData.Vertex vertex1
      {
        get
        {
          return new ModelData.Vertex(this.model, this.model.EdgeIndecies[this.index].index1);
        }
      }

      private ModelData.Vertex vertex2
      {
        get
        {
          return new ModelData.Vertex(this.model, this.model.EdgeIndecies[this.index].index2);
        }
      }
    }

    private class EdgeIndex : IEquatable<ModelData.EdgeIndex>
    {
      private int p1_ID;
      private int p2_ID;

      public EdgeIndex(int p1_ID, int p2_ID)
      {
        this.p1_ID = p1_ID;
        this.p2_ID = p2_ID;
      }

      public List<int> Faces { get; set; }

      public int index1
      {
        get
        {
          return this.p1_ID;
        }
      }

      public int index2
      {
        get
        {
          return this.p2_ID;
        }
      }

      public bool Equals(ModelData.EdgeIndex other)
      {
        if (this.p1_ID == other.p1_ID && this.p2_ID == other.p2_ID)
          return true;
        if (this.p1_ID == other.p2_ID)
          return this.p2_ID == other.p1_ID;
        return false;
      }

      public override int GetHashCode()
      {
        return this.p1_ID * this.p2_ID ^ this.p1_ID + this.p2_ID;
      }

      private int GetHashCode(List<int> list)
      {
        int num1 = 0;
        int num2 = 1;
        int num3 = 0;
        for (int index = 0; index < list.Count; ++index)
        {
          num1 += list[index];
          num2 *= list[index];
          num3 += list[index];
        }
        return num1 ^ num2;
      }

      public override string ToString()
      {
        string str = string.Format("Edge {2} [{0} - {1}]: ", (object) this.p1_ID, (object) this.p2_ID, (object) this.GetHashCode());
        if (this.Faces == null)
        {
          str += "has no faces";
        }
        else
        {
          foreach (int face in this.Faces)
          {
            str += face.ToString();
            if (face != this.Faces.Last<int>())
              str += ", ";
          }
        }
        return str;
      }
    }

    public class Face : IEquatable<ModelData.Face>
    {
      private int index;
      private ModelData ModelData;

      internal Face(ModelData ModelData, int index)
      {
        this.ModelData = ModelData;
        this.index = index;
      }

      public List<ModelData.Vertex> Verticies
      {
        get
        {
          List<ModelData.Vertex> vertexList = new List<ModelData.Vertex>();
          ModelData.FaceIndex faceIndecy = this.ModelData.FaceIndecies[this.index];
          vertexList.Add(this.ModelData.getVertex(faceIndecy.P1));
          vertexList.Add(this.ModelData.getVertex(faceIndecy.P2));
          vertexList.Add(this.ModelData.getVertex(faceIndecy.P3));
          return vertexList;
        }
      }

      public int[] Indicies
      {
        get
        {
          ModelData.FaceIndex faceIndecy = this.ModelData.FaceIndecies[this.index];
          return new int[3]
          {
            faceIndecy.P1,
            faceIndecy.P2,
            faceIndecy.P3
          };
        }
      }

      public int index1
      {
        get
        {
          return this.ModelData.FaceIndecies[this.index].P1;
        }
      }

      public int index2
      {
        get
        {
          return this.ModelData.FaceIndecies[this.index].P2;
        }
      }

      public int index3
      {
        get
        {
          return this.ModelData.FaceIndecies[this.index].P3;
        }
      }

      public M3D.Model.Utils.Vector3 Normal
      {
        get
        {
          return this.ModelData.FaceIndecies[this.index].Normal;
        }
      }

      public int Index
      {
        get
        {
          return this.index;
        }
      }

      public bool Equals(ModelData.Face other)
      {
        return this.index == other.index;
      }
    }

    private class FaceIndex : IEquatable<ModelData.FaceIndex>
    {
      public FaceIndex(int[] faceIndicies, M3D.Model.Utils.Vector3 normal)
        : this(faceIndicies[0], faceIndicies[1], faceIndicies[2], normal)
      {
      }

      public FaceIndex(int P1, int P2, int P3, M3D.Model.Utils.Vector3 normal)
      {
        this.P1 = P1;
        this.P2 = P2;
        this.P3 = P3;
        this.Normal = normal;
      }

      public M3D.Model.Utils.Vector3 Normal { get; private set; }

      public int P1 { get; private set; }

      public int P2 { get; private set; }

      public int P3 { get; private set; }

      public bool Equals(ModelData.FaceIndex other)
      {
        int[] numArray1 = new int[3]
        {
          this.P1,
          this.P2,
          this.P3
        };
        int[] numArray2 = new int[3]
        {
          other.P1,
          other.P2,
          other.P3
        };
        int num1 = 0;
        int num2 = 0;
        for (int index = 0; index < 3; ++index)
        {
          num1 ^= numArray1[index];
          num2 ^= numArray2[index];
        }
        return num1 == num2;
      }
    }

    public class Vertex : IVertex
    {
      private int index;
      private ModelData ModelData;

      internal Vertex(ModelData ModelData, int index)
      {
        this.ModelData = ModelData;
        this.index = index;
      }

      public List<ModelData.Face> Faces
      {
        get
        {
          List<ModelData.Face> faceList = new List<ModelData.Face>();
          foreach (int face in this.ModelData.VertexIndecies[this.index].Faces)
            faceList.Add(this.ModelData.getFace(face));
          return faceList;
        }
      }

      public float x
      {
        get
        {
          return this.ModelData.Verticies[this.index].x;
        }
      }

      public float y
      {
        get
        {
          return this.ModelData.Verticies[this.index].y;
        }
      }

      public float z
      {
        get
        {
          return this.ModelData.Verticies[this.index].z;
        }
      }

      public int Index
      {
        get
        {
          return this.index;
        }
      }

      public double[] Position
      {
        get
        {
          return new double[3]
          {
            (double) this.x,
            (double) this.y,
            (double) this.z
          };
        }
      }
    }

    private class VertexIndex
    {
      public List<int> Faces;

      public VertexIndex(List<int> FaceIndecies)
      {
        this.Faces = new List<int>((IEnumerable<int>) FaceIndecies);
      }
    }
  }
}
