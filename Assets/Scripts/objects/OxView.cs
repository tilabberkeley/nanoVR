using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static GlobalVariables;
using static Utils;

public class OxView : MonoBehaviour
{
    List<GameObject> nucleotides;
    List<GameObject> backbones;
    int nucleotideIndex;
    int backboneIndex;

    public OxView()
    {
        nucleotides = new List<GameObject>();
        backbones = new List<GameObject>();
        nucleotideIndex = 0;
        backboneIndex = 0;
    }

    public async Task BuildStructure(int length)
    {
        // Draw strand.
        // First check if ObjectPool has enough GameObjects to use.
        // If not, generate them async.

        // TODO: Testing this!! - 8/4/24 DY
        int count64 = length / 64;
        if (ObjectPoolManager.Instance.CanGetNucleotides(length) && ObjectPoolManager.Instance.CanGetBackbones(length - 1))
        {
            nucleotides.AddRange(ObjectPoolManager.Instance.GetNucleotides(length));
            backbones.AddRange(ObjectPoolManager.Instance.GetBackbones(length - 1));
        }
        else
        {
            await GenerateGameObjects(length, true);
        }
    }
    private async Task GenerateGameObjects(int length, bool hideGameObjects)
    {
        //int num64nt = length / 64;
        //length %= 64;
        int num32nt = length / 32;
        length %= 32;
        int num16nt = length / 16;
        length %= 16;
        int num8nt = length / 8;
        length %= 8;
        int num4nt = length / 4;
        length %= 4;
        int num2nt = length / 2;
        length %= 2;
        int num1nt = length / 1;

        //int numConnectingBackbones = num64nt + num32nt + num16nt + num8nt + num4nt + num2nt + num1nt - 1;
        int numConnectingBackbones = num32nt + num16nt + num8nt + num4nt + num2nt + num1nt - 1;


        /*for (int i = 0; i < num64nt; i++)
        {
            _nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_64, hideGameObjects));
            await Task.Yield();

            _nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_64, hideGameObjects));
            await Task.Yield();

            _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_63, hideGameObjects));
            await Task.Yield();

            _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_63, hideGameObjects));
            await Task.Yield();

        }*/
        //await Task.Yield();
        for (int i = 0; i < num32nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_32, hideGameObjects));
            await Task.Yield();

            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_32, hideGameObjects));
            //await Task.Yield();

            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_31, hideGameObjects));
            await Task.Yield();

            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_31, hideGameObjects));
            //await Task.Yield();

        }

        for (int i = 0; i < num16nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_16, hideGameObjects));
            //await Task.Yield();

            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_16, hideGameObjects));
            //await Task.Yield();

            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_15, hideGameObjects));
            //await Task.Yield();

            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_15, hideGameObjects));
            //await Task.Yield();
        }


        for (int i = 0; i < num8nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_8, hideGameObjects));
            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_8, hideGameObjects));
            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_7, hideGameObjects));
            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_7, hideGameObjects));
        }
        await Task.Yield();

        for (int i = 0; i < num4nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_4, hideGameObjects));
            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_4, hideGameObjects));
            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_3, hideGameObjects));
            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_3, hideGameObjects));
        }

        for (int i = 0; i < num2nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_2, hideGameObjects));
            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_2, hideGameObjects));
            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
        }

        for (int i = 0; i < num1nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
        }

        for (int i = 0; i < numConnectingBackbones; i++)
        {
            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
        }
        await Task.Yield();
    }
   

    public List<GameObject> GetSubstructure(int length)
    {
        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < length - 1; i++)
        {
            temp.Add(nucleotides[i + nucleotideIndex]);
            temp.Add(backbones[i + backboneIndex]);
        }
        temp.Add(nucleotides[nucleotideIndex + length - 1]);

        // Update indices after GetSubstructure which is last method called.
        nucleotideIndex += length;
        backboneIndex += length - 1;
        return temp;
    }

    public void SetNucleotides(int length, List<Vector3> positions, List<Vector3> a1s, List<int> ids)
    {
        for (int i = 0; i < length; i++)
        {
            Vector3 position = (positions[i] - 0.4f * a1s[i]) / SCALE;
            DrawPoint.SetNucleotide(nucleotides[i + nucleotideIndex], position, ids[i], -1, -1, false, true) ;
            if (i > 0)
            {
                DrawPoint.SetBackbone(backbones[i + backboneIndex - 1], -1, -1, -1, nucleotides[i + nucleotideIndex - 1].transform.position, nucleotides[i + nucleotideIndex].transform.position, false, true);
            }
        }
    }
}
