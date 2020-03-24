﻿using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public class BitmapOperationsUtility : NotifyableObject
    {
        public MouseMovementController MouseController { get; set; }
        public Tool SelectedTool { get; set; }

        private ObservableCollection<Layer> _layers = new ObservableCollection<Layer>();

        public ObservableCollection<Layer> Layers
        {
            get => _layers;
            set { if (_layers != value) { _layers = value; } }
        }
        private int _activeLayerIndex;
        public int ActiveLayerIndex
        {
            get => _activeLayerIndex;
            set
            {
                _activeLayerIndex = value;
                RaisePropertyChanged("ActiveLayerIndex");
                RaisePropertyChanged("ActiveLayer");
            }
        }

        private Layer _previewLayer;

        public Layer PreviewLayer
        {
            get { return _previewLayer; }
            set 
            {
                _previewLayer = value;
                RaisePropertyChanged("PreviewLayer");
            }
        }


        public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveLayerIndex] : null;

        public Color PrimaryColor { get; set; }
        
        public int ToolSize { get; set; }

        public event EventHandler<BitmapChangedEventArgs> BitmapChanged;
        public event EventHandler<LayersChangedEventArgs> LayersChanged;

        private Coordinates _lastMousePos;
        private BitmapPixelChanges _lastChangedPixels;

        public BitmapOperationsUtility()
        {
            MouseController = new MouseMovementController();
            MouseController.MousePositionChanged += Controller_MousePositionChanged;
            MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            if(SelectedTool.RequiresPreviewLayer)
            {
                BitmapPixelChanges oldValues = GetOldPixelsValues(_lastChangedPixels.ChangedPixels.Keys.ToArray());
                Layers[ActiveLayerIndex].ApplyPixels(_lastChangedPixels);
                BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(_lastChangedPixels, oldValues, ActiveLayerIndex));
                _previewLayer.Clear();
            }
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            if(SelectedTool != null && SelectedTool.ToolType != ToolType.None && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var mouseMove = MouseController.LastMouseMoveCoordinates.ToList();
                mouseMove.Reverse();

                if (!SelectedTool.RequiresPreviewLayer)
                {
                    BitmapPixelChanges changedPixels = SelectedTool.Use(Layers[ActiveLayerIndex], mouseMove.ToArray(), PrimaryColor, ToolSize);
                    BitmapPixelChanges oldPixelsValues = GetOldPixelsValues(changedPixels.ChangedPixels.Keys.ToArray());
                    ActiveLayer.ApplyPixels(changedPixels);
                    BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(changedPixels, oldPixelsValues, ActiveLayerIndex));
                }
                else
                {
                    UseToolOnPreviewLayer(mouseMove);
                }

                _lastMousePos = e.NewPosition;
            }
        }

        private BitmapPixelChanges GetOldPixelsValues(Coordinates[] coordinates)
        {
            Dictionary<Coordinates, Color> values = new Dictionary<Coordinates, Color>();
            Layers[ActiveLayerIndex].LayerBitmap.Lock();
            for (int i = 0; i < coordinates.Length; i++)
            {
                values.Add(coordinates[i], Layers[ActiveLayerIndex].LayerBitmap.GetPixel(coordinates[i].X, coordinates[i].Y));
            }
            Layers[ActiveLayerIndex].LayerBitmap.Unlock();
            return new BitmapPixelChanges(values);
        }

        private void UseToolOnPreviewLayer(List<Coordinates> mouseMove)
        {
            BitmapPixelChanges changedPixels;
            if (mouseMove[0] != _lastMousePos)
            {
                GeneratePreviewLayer();
                PreviewLayer.Clear();
                changedPixels = SelectedTool.Use(Layers[ActiveLayerIndex], mouseMove.ToArray(), PrimaryColor, ToolSize);
                PreviewLayer.ApplyPixels(changedPixels);
                _lastChangedPixels = changedPixels;
            }
        }

        private void GeneratePreviewLayer()
        {
            if (PreviewLayer == null)
            {
                PreviewLayer = new Layer("_previewLayer", Layers[0].Width, Layers[0].Height);
            }
        }

        public void AddNewLayer(string name, int width, int height, bool setAsActive = true)
        {
            Layers.Add(new Layer(name, width, height));
            if (setAsActive)
            {
                ActiveLayerIndex = Layers.Count - 1;
            }
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(0, LayerAction.Add));
        }

        public void SetActiveLayer(int index)
        {
            ActiveLayerIndex = index;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(index, LayerAction.SetActive));
        }

        public WriteableBitmap GetCombinedLayersBitmap()
        {
            WriteableBitmap finalBitmap = Layers[0].LayerBitmap.Clone();
            finalBitmap.Lock();
            for (int i = 1; i < Layers.Count; i++)
            {
                for (int y = 0; y < finalBitmap.Height; y++)
                {
                    for (int x = 0; x < finalBitmap.Width; x++)
                    {
                        Color color = Layers[i].LayerBitmap.GetPixel(x, y);
                        if (color.A != 0 || color.R != 0 || color.B != 0 || color.G != 0)
                        {
                            finalBitmap.SetPixel(x, y, color);
                        }
                    }
                }
            }
            finalBitmap.Unlock();
            return finalBitmap;
        }
    }
}

public class BitmapChangedEventArgs : EventArgs
{
    public BitmapPixelChanges PixelsChanged { get; set; }
    public BitmapPixelChanges OldPixelsValues { get; set; }
    public int ChangedLayerIndex { get; set; }

    public BitmapChangedEventArgs(BitmapPixelChanges pixelsChanged, BitmapPixelChanges oldPixelsValues, int changedLayerIndex)
    {
        PixelsChanged = pixelsChanged;
        OldPixelsValues = oldPixelsValues;
        ChangedLayerIndex = changedLayerIndex;
    }
}

public class LayersChangedEventArgs : EventArgs
{
    public int LayerAffected { get; set; }
    public LayerAction LayerChangeType { get; set; }

    public LayersChangedEventArgs(int layerAffected, LayerAction layerChangeType)
    {
        LayerAffected = layerAffected;
        LayerChangeType = layerChangeType;
    }
}
