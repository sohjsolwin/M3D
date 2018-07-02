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
      FaceIndecies = newFaceList;
      Verticies = newVerticesList;
      VertexIndecies = newVertexIndecies;
    }

    public M3D.Model.Utils.Vector3 this[int index]
    {
      get
      {
        return new M3D.Model.Utils.Vector3(Verticies[index]);
      }
    }

    public M3D.Model.Utils.Vector3 Min { private set; get; }

    public M3D.Model.Utils.Vector3 Max { private set; get; }

    public M3D.Model.Utils.Vector3 Ext
    {
      get
      {
        return Max - Min;
      }
    }

    public M3D.Model.Utils.Vector3 Center
    {
      get
      {
        return (Max - Min) / 2f + Min;
      }
    }

    private void ProcessMinMaxBounds()
    {
      var maxValue1 = float.MaxValue;
      var maxValue2 = float.MaxValue;
      var maxValue3 = float.MaxValue;
      var minValue1 = float.MinValue;
      var minValue2 = float.MinValue;
      var minValue3 = float.MinValue;
      List<M3D.Model.Utils.Vector3> convexHullVertices = GetAllConvexHullVertices();
      for (var index = 0; index < convexHullVertices.Count; ++index)
      {
        MinMaxBounds_Helper(ref maxValue1, ref minValue1, convexHullVertices[index].X);
        MinMaxBounds_Helper(ref maxValue2, ref minValue2, convexHullVertices[index].Y);
        MinMaxBounds_Helper(ref maxValue3, ref minValue3, convexHullVertices[index].Z);
      }
      Min = new M3D.Model.Utils.Vector3(maxValue1, maxValue2, maxValue3);
      Max = new M3D.Model.Utils.Vector3(minValue1, minValue2, minValue3);
    }

    public void GetMinMaxWithTransform(Matrix4 matrix, out M3D.Model.Utils.Vector3 min, out M3D.Model.Utils.Vector3 max)
    {
      var maxValue1 = float.MaxValue;
      var maxValue2 = float.MaxValue;
      var maxValue3 = float.MaxValue;
      var minValue1 = float.MinValue;
      var minValue2 = float.MinValue;
      var minValue3 = float.MinValue;
      List<M3D.Model.Utils.Vector3> convexHullVertices = GetAllConvexHullVertices();
      for (var index = 0; index < convexHullVertices.Count; ++index)
      {
        var vector3 = M3D.Model.Utils.Vector3.MatrixProduct(matrix, convexHullVertices[index]);
        MinMaxBounds_Helper(ref maxValue1, ref minValue1, vector3.X);
        MinMaxBounds_Helper(ref maxValue2, ref minValue2, vector3.Y);
        MinMaxBounds_Helper(ref maxValue3, ref minValue3, vector3.Z);
      }
      min = new M3D.Model.Utils.Vector3(maxValue1, maxValue2, maxValue3);
      max = new M3D.Model.Utils.Vector3(minValue1, minValue2, minValue3);
    }

    private void MinMaxBounds_Helper(ref float min, ref float max, float var)
    {
      if (min > (double)var)
      {
        min = var;
      }

      if (max >= (double)var)
      {
        return;
      }

      max = var;
    }

    public List<ModelData.Vertex> GetAllVertexs()
    {
      var vertexList = new List<ModelData.Vertex>(Verticies.Count<M3D.Model.Utils.Vector3>());
      for (var index = 0; index < Verticies.Count; ++index)
      {
        vertexList.Add(GetVertex(index));
      }

      return vertexList;
    }

    public List<ModelData.Face> GetAllFaces()
    {
      var faceList = new List<ModelData.Face>(FaceIndecies.Count);
      for (var index = 0; index < FaceIndecies.Count; ++index)
      {
        faceList.Add(GetFace(index));
      }

      return faceList;
    }

    public ModelData.Vertex GetVertex(int index)
    {
      return new ModelData.Vertex(this, index);
    }

    public ModelData.Face GetFace(int index)
    {
      return new ModelData.Face(this, index);
    }

    public int GetVertexCount()
    {
      return Verticies.Count;
    }

    public int GetFaceCount()
    {
      return FaceIndecies.Count;
    }

    public void Translate(M3D.Model.Utils.Vector3 translate)
    {
      for (var index = 0; index < Verticies.Count; ++index)
      {
        Verticies[index] += translate;
      }

      Min += translate;
      Max += translate;
    }

    public void Scale(M3D.Model.Utils.Vector3 scale)
    {
      for (var index = 0; index < Verticies.Count; ++index)
      {
        Verticies[index] *= scale;
      }

      Min *= scale;
      Max *= scale;
    }

    public void Transform(Matrix4 transform)
    {
      for (var index = 0; index < Verticies.Count; ++index)
      {
        Verticies[index].MatrixProduct(transform);
      }

      ProcessMinMaxBounds();
    }

    private void Clear()
    {
      FaceIndecies.Clear();
      VertexIndecies.Clear();
      Verticies.Clear();
    }

    private void InitalizeConvexHull(List<ModelData.Vertex> points)
    {
      ConvexHullIndicies = new HashSet<int>(collection: ConvexHull.Create(points.ConvertAll(x => (IVertex)x)).Points.ToList().ConvertAll(x => ((Vertex)x).Index));
    }

    public List<M3D.Model.Utils.Vector3> GetAllConvexHullVertices()
    {
      var vector3List = new List<M3D.Model.Utils.Vector3>(ConvexHullIndicies.Count);
      foreach (var convexHullIndicy in ConvexHullIndicies)
      {
        vector3List.Add(Verticies[convexHullIndicy]);
      }

      return vector3List;
    }

    public List<M3D.Model.Utils.Vector3> CalculateHullPointsUsingTransformMatrix(Matrix4 matrix)
    {
      var vector3List = new List<M3D.Model.Utils.Vector3>(ConvexHullIndicies.Count);
      foreach (var convexHullIndicy in ConvexHullIndicies)
      {
        var vector3 = new M3D.Model.Utils.Vector3(Verticies[convexHullIndicy]);
        vector3.MatrixProduct(matrix);
        vector3List.Add(vector3);
      }
      return vector3List;
    }

    public bool IsConvexHullPoint(int VertexIndex)
    {
      return ConvexHullIndicies.Contains(VertexIndex);
    }

    internal static ModelData Create(LinkedList<M3D.Model.Utils.Vector3> verticies, LinkedList<int[]> triangleIndecies = null, ProgressHelper.PercentageDelagate percentageDeligate = null)
    {
      var stopwatch = new Stopwatch();
      stopwatch.Start();
      var progressHelper = new ProgressHelper(percentageDeligate, 3);
      if (triangleIndecies != null)
      {
        ModelData.GenerateOrderedList(ref verticies, triangleIndecies);
      }

      if (verticies.Count % 3 != 0)
      {
        return null;
      }

      progressHelper.SetStage(verticies.Count<M3D.Model.Utils.Vector3>());
      if (!ModelData.HashVertexesAndFaces_Helper(verticies, ref progressHelper, out List<Utils.Vector3> newVerticesList, out List<FaceIndex> newFaceList))
      {
        return null;
      }

      progressHelper.SetStage(newFaceList.Count<ModelData.FaceIndex>());
      List<ModelData.VertexIndex> faceLinkHelper = ModelData.GenerateFaceLink_Helper(newVerticesList, newFaceList, progressHelper);
      var modelData = new ModelData(newFaceList, newVerticesList, faceLinkHelper);
      modelData.InitalizeConvexHull(modelData.GetAllVertexs());
      modelData.ProcessMinMaxBounds();
      stopwatch.Stop();
      if (percentageDeligate == null)
      {
        return modelData;
      }

      percentageDeligate(100);
      return modelData;
    }

    private static void GenerateOrderedList(ref LinkedList<M3D.Model.Utils.Vector3> verticesList, LinkedList<int[]> faceIndicesList)
    {
      M3D.Model.Utils.Vector3[] vector3Array = new M3D.Model.Utils.Vector3[verticesList.Count];
      LinkedListNode<M3D.Model.Utils.Vector3> linkedListNode1 = verticesList.First;
      for (var index = 0; index < verticesList.Count; ++index)
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
            foreach (var index in numArray)
            {
              verticesList.AddLast(vector3Array[index]);
            }

            continue;
          case 4:
            for (var index = 0; index < 3; ++index)
            {
              verticesList.AddLast(vector3Array[index]);
            }

            for (var index = 0; index < 3; ++index)
            {
              verticesList.AddLast(vector3Array[index + 1]);
            }

            continue;
          default:
            throw new Exception(string.Format("ExpandVertices: was give a face with {0} indicies", numArray.Length));
        }
      }
    }

    private static bool HashVertexesAndFaces_Helper(LinkedList<M3D.Model.Utils.Vector3> verticies, ref ProgressHelper progressHelper, out List<M3D.Model.Utils.Vector3> newVerticesList, out List<ModelData.FaceIndex> newFaceList)
    {
      newVerticesList = null;
      newFaceList = null;
      var num1 = 0;
      var num2 = 0;
      var source = new Dictionary<M3D.Model.Utils.Vector3, int>();
      var faceIndexSet = new HashSet<ModelData.FaceIndex>();
      var flag = ModelData.VerticiesFlipped(verticies);
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
        if (!double.IsNaN(normal.X) && !double.IsNaN(normal.Y) && !double.IsNaN(normal.Z))
        {
          int[] faceIndicies = new int[3];
          M3D.Model.Utils.Vector3[] vector3Array = new M3D.Model.Utils.Vector3[3]
          {
            vector3_1,
            vector3_2,
            vector3_3
          };
          for (var index = 0; index < 3; ++index)
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
          for (var index1 = 0; index1 < 3; ++index1)
          {
            for (var index2 = index1 + 1; index2 < 3; ++index2)
            {
              if (faceIndicies[index1] == faceIndicies[index2])
              {
                return false;
              }
            }
          }
          var faceIndex = new FaceIndex(faceIndicies, normal);
          if (!faceIndexSet.Contains(faceIndex))
          {
            faceIndexSet.Add(faceIndex);
          }

          progressHelper.Process(num2 += 3);
        }
      }
      IEnumerable<M3D.Model.Utils.Vector3> collection = source.OrderBy<KeyValuePair<M3D.Model.Utils.Vector3, int>, int>(pair => pair.Value).Select<KeyValuePair<M3D.Model.Utils.Vector3, int>, M3D.Model.Utils.Vector3>(pair => pair.Key);
      newVerticesList = new List<M3D.Model.Utils.Vector3>(collection);
      source.Clear();
      newFaceList = new List<ModelData.FaceIndex>(faceIndexSet);
      faceIndexSet.Clear();
      return true;
    }

    private static void AssertIfNANOrNULL(M3D.Model.Utils.Vector3 p1)
    {
      if (p1 == null)
      {
        throw new NullReferenceException("Null point in loading");
      }

      if (double.IsNaN(p1.X) || double.IsNaN(p1.Y) || double.IsNaN(p1.Z))
      {
        throw new Exception("Point contains a NAN");
      }
    }

    private static List<ModelData.VertexIndex> GenerateFaceLink_Helper(List<M3D.Model.Utils.Vector3> newVerticesList, List<ModelData.FaceIndex> newFaceList, ProgressHelper progressHelper = null)
    {
      var vertexIndexList = new List<ModelData.VertexIndex>(newFaceList.Count);
      foreach (M3D.Model.Utils.Vector3 newVertices in newVerticesList)
      {
        vertexIndexList.Add(new ModelData.VertexIndex(new List<int>()));
      }

      for (var index = 0; index < newFaceList.Count; ++index)
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
      var hashCollection = new Dictionary<ModelData.EdgeIndex, ModelData.EdgeIndex>();
      for (var index = 0; index < faceList.Count; ++index)
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
      var key = new ModelData.EdgeIndex(P1_ID, P2_ID);
      if (!hashCollection.ContainsKey(key))
      {
        key.Faces = new List<int>() { faceId };
        hashCollection.Add(key, key);
      }
      else
      {
        ModelData.EdgeIndex edgeIndex = hashCollection[key];
        if (edgeIndex.Faces.Contains(faceId))
        {
          return;
        }

        edgeIndex.Faces.Add(faceId);
      }
    }

    private static bool CommonWindingTest_Helper(int[] face1, int[] face2, int[] common)
    {
      var num1 = 0;
      var num2 = 0;
      for (var index = 0; index < 3; ++index)
      {
        if (num1 == 0)
        {
          if (face1[index] == common[0])
          {
            num1 = -1;
          }
          else if (face1[index] == common[1])
          {
            num1 = 1;
          }
        }
        if (num2 == 0)
        {
          if (face2[index] == common[0])
          {
            num2 = -1;
          }
          else if (face2[index] == common[1])
          {
            num2 = 1;
          }
        }
      }
      return num1 + num2 == 0;
    }

    private static bool VerticiesFlipped(List<ModelData.FaceIndex> faces, List<M3D.Model.Utils.Vector3> vertices)
    {
      return ModelData.GenerateSignedVolume(faces, vertices) < 0.0;
    }

    private static double GenerateSignedVolume(List<ModelData.FaceIndex> faces, List<M3D.Model.Utils.Vector3> vertices)
    {
      var num1 = 0.0;
      for (var index = 0; index < faces.Count; ++index)
      {
        M3D.Model.Utils.Vector3 vertex1 = vertices[faces[index].P1];
        M3D.Model.Utils.Vector3 vertex2 = vertices[faces[index].P2];
        M3D.Model.Utils.Vector3 vertex3 = vertices[faces[index].P3];
        var num2 = vertex1.X * (double) vertex2.Y * vertex3.Z + vertex1.Y * (double)vertex2.Z * vertex3.X + vertex1.Z * (double)vertex2.X * vertex3.Y - vertex1.Z * (double)vertex2.Y * vertex3.X - vertex1.Y * (double)vertex2.X * vertex3.Z - vertex1.X * (double)vertex2.Z * vertex3.Y;
        num1 += num2 / 6.0;
      }
      return num1;
    }

    private static bool VerticiesFlipped(LinkedList<M3D.Model.Utils.Vector3> linkedVertex)
    {
      var num1 = 0.0;
      LinkedListNode<M3D.Model.Utils.Vector3> linkedListNode = linkedVertex.First;
      while (linkedListNode != null)
      {
        M3D.Model.Utils.Vector3 vector3_1 = linkedListNode.Value;
        LinkedListNode<M3D.Model.Utils.Vector3> next1 = linkedListNode.Next;
        M3D.Model.Utils.Vector3 vector3_2 = next1.Value;
        LinkedListNode<M3D.Model.Utils.Vector3> next2 = next1.Next;
        M3D.Model.Utils.Vector3 vector3_3 = next2.Value;
        linkedListNode = next2.Next;
        var num2 = vector3_1.X * (double)vector3_2.Y * vector3_3.Z + vector3_1.Y * (double)vector3_2.Z * vector3_3.X + vector3_1.Z * (double)vector3_2.X * vector3_3.Y - vector3_1.Z * (double)vector3_2.Y * vector3_3.X - vector3_1.Y * (double)vector3_2.X * vector3_3.Z - vector3_1.X * (double)vector3_2.Z * vector3_3.Y;
        num1 += num2 / 6.0;
      }
      return num1 < 0.0;
    }

    public static M3D.Model.Utils.Vector3 CalcNormal(M3D.Model.Utils.Vector3 _a, M3D.Model.Utils.Vector3 _b, M3D.Model.Utils.Vector3 _c)
    {
      var num1 = _b.X - _a.X;
      var num2 = _b.Y - _a.Y;
      var num3 = _b.Z - _a.Z;
      var num4 = _c.X - _a.X;
      var num5 = _c.Y - _a.Y;
      var num6 = _c.Z - _a.Z;
      var num7 = num2 * (double)num6 - num3 * (double)num5;
      var num8 = (float)(num3 * (double)num4 - num1 * (double)num6);
      var num9 = (float)(num1 * (double)num5 - num2 * (double)num4);
      var num10 = (float) Math.Sqrt(num7 * num7 + num8 * (double)num8 + num9 * (double)num9);
      return new M3D.Model.Utils.Vector3((float) num7 / num10, num8 / num10, num9 / num10);
    }

    public static ModelData Stitch(List<ModelTransform> modelTransforms)
    {
      var capacity1 = 0;
      var capacity2 = 0;
      foreach (ModelTransform modelTransform in modelTransforms)
      {
        capacity1 += modelTransform.data.GetFaceCount();
        capacity2 += modelTransform.data.GetVertexCount();
      }
      var newFaceList = new List<ModelData.FaceIndex>(capacity1);
      var newVerticesList = new List<M3D.Model.Utils.Vector3>(capacity2);
      var num1 = 0;
      var num2 = 0;
      foreach (ModelTransform modelTransform in modelTransforms)
      {
        var faceCount = modelTransform.data.GetFaceCount();
        var vertexCount = modelTransform.data.GetVertexCount();
        for (var index = 0; index < faceCount; ++index)
        {
          ModelData.Face face = modelTransform.data.GetFace(index);
          newFaceList.Add(new ModelData.FaceIndex(face.Index1 + num2, face.Index2 + num2, face.Index3 + num2, face.Normal));
        }
        for (var index = 0; index < vertexCount; ++index)
        {
          var vector3 = Utils.Vector3.MatrixProduct(modelTransform.transformMatrix, modelTransform.data[index]);
          newVerticesList.Add(vector3);
        }
        num1 += faceCount;
        num2 += vertexCount;
      }
      List<ModelData.VertexIndex> faceLinkHelper = ModelData.GenerateFaceLink_Helper(newVerticesList, newFaceList, null);
      return new ModelData(newFaceList, newVerticesList, faceLinkHelper);
    }

    private class Edge : IEquatable<ModelData.Edge>
    {
      private int index;
      private ModelData model;

      public bool Equals(ModelData.Edge other)
      {
        return index == other.index;
      }

      private List<ModelData.Face> GetFaces
      {
        get
        {
          var faceList = new List<ModelData.Face>();
          foreach (var face in model.EdgeIndecies[index].Faces)
          {
            faceList.Add(new ModelData.Face(model, face));
          }

          return faceList;
        }
      }

      private ModelData.Vertex Vertex1
      {
        get
        {
          return new ModelData.Vertex(model, model.EdgeIndecies[index].Index1);
        }
      }

      private ModelData.Vertex Vertex2
      {
        get
        {
          return new ModelData.Vertex(model, model.EdgeIndecies[index].Index2);
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

      public int Index1
      {
        get
        {
          return p1_ID;
        }
      }

      public int Index2
      {
        get
        {
          return p2_ID;
        }
      }

      public bool Equals(ModelData.EdgeIndex other)
      {
        if (p1_ID == other.p1_ID && p2_ID == other.p2_ID)
        {
          return true;
        }

        if (p1_ID == other.p2_ID)
        {
          return p2_ID == other.p1_ID;
        }

        return false;
      }

      public override int GetHashCode()
      {
        return p1_ID * p2_ID ^ p1_ID + p2_ID;
      }

      private int GetHashCode(List<int> list)
      {
        var num1 = 0;
        var num2 = 1;
        var num3 = 0;
        for (var index = 0; index < list.Count; ++index)
        {
          num1 += list[index];
          num2 *= list[index];
          num3 += list[index];
        }
        return num1 ^ num2;
      }

      public override string ToString()
      {
        var str = string.Format("Edge {2} [{0} - {1}]: ", p1_ID, p2_ID, GetHashCode());
        if (Faces == null)
        {
          str += "has no faces";
        }
        else
        {
          foreach (var face in Faces)
          {
            str += face.ToString();
            if (face != Faces.Last<int>())
            {
              str += ", ";
            }
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
          var vertexList = new List<ModelData.Vertex>();
          ModelData.FaceIndex faceIndecy = ModelData.FaceIndecies[index];
          vertexList.Add(ModelData.GetVertex(faceIndecy.P1));
          vertexList.Add(ModelData.GetVertex(faceIndecy.P2));
          vertexList.Add(ModelData.GetVertex(faceIndecy.P3));
          return vertexList;
        }
      }

      public int[] Indicies
      {
        get
        {
          ModelData.FaceIndex faceIndecy = ModelData.FaceIndecies[index];
          return new int[3]
          {
            faceIndecy.P1,
            faceIndecy.P2,
            faceIndecy.P3
          };
        }
      }

      public int Index1
      {
        get
        {
          return ModelData.FaceIndecies[index].P1;
        }
      }

      public int Index2
      {
        get
        {
          return ModelData.FaceIndecies[index].P2;
        }
      }

      public int Index3
      {
        get
        {
          return ModelData.FaceIndecies[index].P3;
        }
      }

      public M3D.Model.Utils.Vector3 Normal
      {
        get
        {
          return ModelData.FaceIndecies[index].Normal;
        }
      }

      public int Index
      {
        get
        {
          return index;
        }
      }

      public bool Equals(ModelData.Face other)
      {
        return index == other.index;
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
        Normal = normal;
      }

      public M3D.Model.Utils.Vector3 Normal { get; private set; }

      public int P1 { get; private set; }

      public int P2 { get; private set; }

      public int P3 { get; private set; }

      public bool Equals(ModelData.FaceIndex other)
      {
        int[] numArray1 = new int[3]
        {
          P1,
          P2,
          P3
        };
        int[] numArray2 = new int[3]
        {
          other.P1,
          other.P2,
          other.P3
        };
        var num1 = 0;
        var num2 = 0;
        for (var index = 0; index < 3; ++index)
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
          var faceList = new List<ModelData.Face>();
          foreach (var face in ModelData.VertexIndecies[index].Faces)
          {
            faceList.Add(ModelData.GetFace(face));
          }

          return faceList;
        }
      }

      public float X
      {
        get
        {
          return ModelData.Verticies[index].X;
        }
      }

      public float Y
      {
        get
        {
          return ModelData.Verticies[index].Y;
        }
      }

      public float Z
      {
        get
        {
          return ModelData.Verticies[index].Z;
        }
      }

      public int Index
      {
        get
        {
          return index;
        }
      }

      public double[] Position
      {
        get
        {
          return new double[3]
          {
             X,
             Y,
             Z
          };
        }
      }
    }

    private class VertexIndex
    {
      public List<int> Faces;

      public VertexIndex(List<int> FaceIndecies)
      {
        Faces = new List<int>(FaceIndecies);
      }
    }
  }
}
