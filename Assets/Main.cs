using System;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using System.Linq;
using UnityEngine.UI;

namespace SeatFinder
{
    public struct UserPreference
    {
        // -1, 0 or 1
        // -1 = close, 0 = medium, 1 = far
        public int temperaturePreference;
        public int noisePreference;
        public int windowPreference;
        
        // false if no preference
        public bool outletPreference;
        
        public UserPreference(int tempVal, int noiseVal, int windowVal, bool outletVal)
        {
            temperaturePreference = tempVal;
            noisePreference = noiseVal;
            windowPreference = windowVal;
            outletPreference = outletVal;
        }
    }
    
    public class Main : MonoBehaviour
    {
        private Transform _seatsContainer;
        
        private List<SeatBehaviour> _seatBehavioursList;
        private List<int> _sortedSeatsByAcDist;
        private List<int> _sortedSeatsByNoiseSrcDist;
        private List<int> _sortedSeatsByWindowDist;
        
        private Transform _airconditionersContainer;
        private Transform _windowsContainer;
        private Transform _noiseSourcesContainer;
        private Transform _outletsContainer;
        
        private float _outletPresenceThresholdDistance;
        
        private void Start()
        {
            _seatsContainer = GameObject.Find("Seats").transform;
            
            _airconditionersContainer = GameObject.Find("Airconditioners").transform;
            _windowsContainer = GameObject.Find("Windows").transform;
            _outletsContainer = GameObject.Find("Outlets").transform;
            _noiseSourcesContainer = GameObject.Find("NoiseSources").transform;

            _seatBehavioursList = new List<SeatBehaviour>();
            _outletPresenceThresholdDistance = 1.5f;
            
            calculateSeatDistances();
           
        }

        private void calculateSeatDistances()
        {
             int seatNum = _seatsContainer.childCount;

            // calculate seat scores
            for (int seatIndex = 0; seatIndex < seatNum; seatIndex++)
            {
                Transform seat = _seatsContainer.GetChild(seatIndex);
                SeatBehaviour seatBehaviour = seat.GetComponent<SeatBehaviour>();
                // ac distances
                for (int acIndex = 0; acIndex < _airconditionersContainer.childCount; acIndex++)
                {
                    Destroy(_airconditionersContainer.GetChild(acIndex).gameObject.GetComponent<MeshRenderer>());
                    Vector3 acPos = _airconditionersContainer.GetChild((acIndex)).transform.position;
                    float dist = (seat.position - acPos).magnitude;

                    if (dist < seatBehaviour.ClosestAcDist)
                    {
                        seatBehaviour.ClosestAcDist = dist;
                        seatBehaviour.ClosestAcPos = acPos;
                    }
                }

                // window distances
                for (int windowIndex = 0; windowIndex < _windowsContainer.childCount; windowIndex++)
                {
                    Destroy(_windowsContainer.GetChild(windowIndex).gameObject.GetComponent<MeshRenderer>());

                    UnityEngine.Vector3 windowPos = _windowsContainer.GetChild((windowIndex)).transform.position;
                    float dist = (seat.position - windowPos).magnitude;

                    seatBehaviour.ClosestWindowDist = dist < seatBehaviour.ClosestWindowDist
                        ? dist
                        : seatBehaviour.ClosestWindowDist;
                }

                // noise source distances
                for (int noiseSrcIndex = 0; noiseSrcIndex < _noiseSourcesContainer.childCount; noiseSrcIndex++)
                {
                    Destroy(_noiseSourcesContainer.GetChild(noiseSrcIndex).gameObject.GetComponent<MeshRenderer>());
                    UnityEngine.Vector3 noiseSrcPos =
                        _noiseSourcesContainer.GetChild((noiseSrcIndex)).transform.position;
                    float dist = (seat.position - noiseSrcPos).magnitude;

                    seatBehaviour.ClosestNoiseSrcDist = dist < seatBehaviour.ClosestNoiseSrcDist
                        ? dist
                        : seatBehaviour.ClosestNoiseSrcDist;
                }

                // outlet presence
                for (int outletIndex = 0; outletIndex < _outletsContainer.childCount; outletIndex++)
                {
                    Destroy(_outletsContainer.GetChild(outletIndex).gameObject.GetComponent<MeshRenderer>());
                    UnityEngine.Vector3 outletPos = _outletsContainer.GetChild((outletIndex)).transform.position;
                    float dist = (seat.position - outletPos).magnitude;

                    // set to present if closer than threshold, otherwise leave as is
                    seatBehaviour.OutletsPresent =
                        dist < _outletPresenceThresholdDistance || seatBehaviour.OutletsPresent;
                }




                // GameObject lineObject = new GameObject("Line");
                // LineRenderer lr = lineObject.AddComponent<LineRenderer>();
                // lr.endColor = Color.blue;
                // lr.startColor = Color.black;
                // Vector3[] positions = new Vector3[3];
                // positions[0] = seat.position;
                // positions[1] = seatBehaviour.ClosestAcPos;
                // lr.positionCount = 2;
                // lr.SetPositions(positions);
                // lr.startWidth = 0.003f;
                // lr.endWidth = 0.03f;
                // Debug.Log("Seat " + seatIndex + " has AC dist " + seatBehaviour.ClosestAcDist + " and window dist " + seatBehaviour.ClosestWindowDist + " and noise dist " + seatBehaviour.ClosestNoiseSrcDist);

                _seatBehavioursList.Add(seatBehaviour);
                seatBehaviour.updateIcons();
            }

            // create 3 sorted lists from low dist to large dist for each parameter
            // outlet doesn't need list, boolean is enough
            // actually arrays of indices in main list is enough for the params
            
            // when picking seat, loop through list, look at index position in the different arrays,
            // if user wants low distance, the index should have low offset from beginning of array, otherwise end,
            // if user wants medium distance the offset to the center of array in either direction should be low
            // add together the distances from the ideal (start, end or middle) and filter out those without outlet if user wants one
            // or maybe rank them lower when finally creating a last array of indices from best (= lowsest summed up offsets) to worst
            // then show user top 2 or 3 of the list 
            
            _sortedSeatsByAcDist = _seatBehavioursList
                .Select((item, index) => new { Item = item, Index = index })
                .OrderBy(x => x.Item.ClosestAcDist)
                .Select(x => x.Index)
                .ToList();
            
            _sortedSeatsByNoiseSrcDist = _seatBehavioursList
                .Select((item, index) => new { Item = item, Index = index })
                .OrderBy(x => x.Item.ClosestNoiseSrcDist)
                .Select(x => x.Index)
                .ToList();
            
           _sortedSeatsByWindowDist = _seatBehavioursList
                .Select((item, index) => new { Item = item, Index = index })
                .OrderBy(x => x.Item.ClosestWindowDist)
                .Select(x => x.Index)
                .ToList();
        }

        private List<(int,int)> findBestSeats(UserPreference userPreference)
        {
            List<(int,int)> bestSeatIndicesAndCombinedOffsets = new List<(int,int)>();
            
            
            for (var i = 0; i < _seatBehavioursList.Count; i++)
            {
                int acOffset;
                int noiseSrcOffset;
                int windowOffset;
                
                int acIndexPos = _sortedSeatsByAcDist.IndexOf(i);
                int noiseSrcIndexPos = _sortedSeatsByNoiseSrcDist.IndexOf(i);
                int windowIndexPos = _sortedSeatsByWindowDist.IndexOf(i);
                bool outletPresent = _seatBehavioursList[i].OutletsPresent;
                
                if (userPreference.temperaturePreference == -1)
                {
                    acOffset = acIndexPos;
                }
                else if (userPreference.temperaturePreference == 0)
                {
                    acOffset = (int)Math.Abs(Math.Floor(_sortedSeatsByAcDist.Count / 2f) - acIndexPos);
                }
                else
                {
                    acOffset = _sortedSeatsByAcDist.Count-1 - acIndexPos;
                }

                if (userPreference.windowPreference == -1)
                {
                    windowOffset = windowIndexPos;
                }
                else if (userPreference.windowPreference == 0)
                {
                    windowOffset = (int)Math.Abs(Math.Floor(_sortedSeatsByWindowDist.Count / 2f) - windowIndexPos);
                }
                else
                {
                    windowOffset = _sortedSeatsByWindowDist.Count-1 - windowIndexPos;
                }

                if (userPreference.noisePreference == -1)
                {
                    noiseSrcOffset = noiseSrcIndexPos;
                }
                else if (userPreference.noisePreference == 0)
                {
                    noiseSrcOffset = (int)Math.Abs(Math.Floor(_sortedSeatsByNoiseSrcDist.Count / 2f) - noiseSrcIndexPos);
                }
                else
                {
                    noiseSrcOffset = _sortedSeatsByNoiseSrcDist.Count-1 - noiseSrcIndexPos;
                }

                int outletOffset = 0;
                if (userPreference.outletPreference && !outletPresent)
                {
                    outletOffset = 4;
                }
                int combineOffset = acOffset + windowOffset + noiseSrcOffset + outletOffset;
                bestSeatIndicesAndCombinedOffsets.Add((i, combineOffset));
                // Debug.Log("Seat " + i + " has offset " + combineOffset);
            }

            // Debug.Log(bestSeatIndicesAndCombinedOffsets.Count);
            
            return bestSeatIndicesAndCombinedOffsets
                .OrderBy(x => x.Item2)
                .ToList();
        }
 
        public void showBestSeats(UserPreference userPreference)
        {
            List<(int,int)> seats = findBestSeats(userPreference);
            for (int i = 0; i < seats.Count; i++)
            {

                if (i < 3)
                {
                    _seatBehavioursList[seats[i].Item1].showSuggestionArrow();
                }
            }
        }

        public void hideBestSeats()
        {
            // just hide arrow for all seats
            for (int i = 0; i < _seatBehavioursList.Count; i++)
            {
                _seatBehavioursList[i].HideArrow();
            }
        }
    }
} 