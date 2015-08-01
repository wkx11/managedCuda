﻿//	Copyright (c) 2012, Michael Kunz. All rights reserved.
//	http://kunzmi.github.io/managedcuda
//
//	This file is part of ManagedCuda.
//
//	ManagedCuda is free software: you can redistribute it and/or modify
//	it under the terms of the GNU Lesser General Public License as 
//	published by the Free Software Foundation, either version 2.1 of the 
//	License, or (at your option) any later version.
//
//	ManagedCuda is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//	GNU Lesser General Public License for more details.
//
//	You should have received a copy of the GNU Lesser General Public
//	License along with this library; if not, write to the Free Software
//	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
//	MA 02110-1301  USA, http://www.gnu.org/licenses/.


using System;
using System.Text;
using System.Diagnostics;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;

namespace ManagedCuda.CudaSparse
{
	/// <summary>
	/// Wrapper class for cusparseContext. Provides all fundamental API functions as methods.
	/// </summary>
	public class CudaSparseContext : IDisposable
	{		
        private cusparseContext _handle;
        private cusparseStatus res;
        private bool disposed;

        #region Contructors
        /// <summary>
        /// Creates a new CudaSparseContext
        /// </summary>
        public CudaSparseContext()
        {
            _handle = new cusparseContext();
            res = CudaSparseNativeMethods.cusparseCreate(ref _handle);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCreate", res));
            if (res != cusparseStatus.Success)
                throw new CudaSparseException(res);
        }
		
        /// <summary>
        /// Creates a new CudaSparseContext and sets the cuda stream to use
        /// </summary>
		/// <param name="stream">A valid CUDA stream created with cudaStreamCreate() (or 0 for the default stream)</param>
        public CudaSparseContext(CUstream stream)
        {
            _handle = new cusparseContext();
            res = CudaSparseNativeMethods.cusparseCreate(ref _handle);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCreate", res));
            if (res != cusparseStatus.Success)
                throw new CudaSparseException(res);
			SetStream(stream);
        }

        /// <summary>
        /// For dispose
        /// </summary>
		~CudaSparseContext()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// For IDisposable
        /// </summary>
        /// <param name="fDisposing"></param>
        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !disposed)
            {
                //Ignore if failing
				res = CudaSparseNativeMethods.cusparseDestroy(_handle);
				Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDestroy", res));
                disposed = true;
            }
            if (!fDisposing && !disposed)
                Debug.WriteLine(String.Format("ManagedCUDA not-disposed warning: {0}", this.GetType()));
        }
        #endregion

		#region Methods
        /// <summary>
		/// Sets the cuda stream to use
        /// </summary>
        /// <param name="stream">A valid CUDA stream created with cudaStreamCreate() (or 0 for the default stream)</param>
        public void SetStream(CUstream stream)
        {
            res = CudaSparseNativeMethods.cusparseSetStream(_handle, stream);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSetStream", res));
            if (res != cusparseStatus.Success)
                throw new CudaSparseException(res);
        }

        /// <summary>
		/// Returns the version of the underlying CUSPARSE library
        /// </summary>
        public Version GetVersion()
        {
			int version = 0;
			res = CudaSparseNativeMethods.cusparseGetVersion(_handle, ref version);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseGetVersion", res));
            if (res != cusparseStatus.Success)
                throw new CudaSparseException(res);
			return new Version((int)version / 1000, (int)version % 100);
        }

		/// <summary>
		/// Returns the pointer mode for scalar values (host or device pointer)
		/// </summary>
		/// <returns></returns>
		public cusparsePointerMode GetPointerMode()
		{
			cusparsePointerMode pointerMode = new cusparsePointerMode();
			res = CudaSparseNativeMethods.cusparseGetPointerMode(_handle, ref pointerMode);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseGetPointerMode", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return pointerMode;
		}

		/// <summary>
		/// Sets the pointer mode for scalar values (host or device pointer)
		/// </summary>
		/// <param name="pointerMode"></param>
		public void SetPointerMode(cusparsePointerMode pointerMode)
		{
			res = CudaSparseNativeMethods.cusparseSetPointerMode(_handle, pointerMode);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSetPointerMode", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		

		/// <summary>
		/// This function returns the number of levels and the assignment of rows into the levels
		/// computed by either the csrsv_analysis, csrsm_analysis or hybsv_analysis routines.
		/// </summary>
		/// <param name="info">the pointer to the solve and analysis structure.</param>
		/// <param name="nLevels">number of levels.</param>
		/// <param name="levelPtr">integer array of nlevels+1 elements that contains
		/// the start of every level and the end of the last
		/// level plus one.</param>
		/// <param name="levelIdx">integer array of m (number of rows in the matrix)
		/// elements that contains the row indices belonging
		/// to every level.</param>
		public void GetLevelInfo(CudaSparseSolveAnalysisInfo info, out int nLevels, out CudaDeviceVariable<int> levelPtr, out CudaDeviceVariable<int> levelIdx)
		{
			nLevels = 0;
			CUdeviceptr levelptr = new CUdeviceptr();
			CUdeviceptr levelidx = new CUdeviceptr();
			res = CudaSparseNativeMethods.cusparseGetLevelInfo(_handle, info.SolveAnalysisInfo, ref nLevels, ref levelptr, ref levelidx);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseGetLevelInfo", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

			levelPtr = new CudaDeviceVariable<int>(levelptr, false);
			levelIdx = new CudaDeviceVariable<int>(levelidx, false);
		}

		#endregion

		#region Sparse Level 1 routines
		/// <summary>
		/// Addition of a scalar multiple of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Axpyi(float alpha, CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<float> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSaxpyi(_handle, (int)xInd.Size, ref alpha, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSaxpyi", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Addition of a scalar multiple of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Axpyi(double alpha, CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<double> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDaxpyi(_handle, (int)xInd.Size, ref alpha, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDaxpyi", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Addition of a scalar multiple of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Axpyi(cuFloatComplex alpha, CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuFloatComplex> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCaxpyi(_handle, (int)xInd.Size, ref alpha, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCaxpyi", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Addition of a scalar multiple of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Axpyi(cuDoubleComplex alpha, CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuDoubleComplex> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZaxpyi(_handle, (int)xInd.Size, ref alpha, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZaxpyi", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// Addition of a scalar multiple of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Axpyi(CudaDeviceVariable<float> alpha, CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<float> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSaxpyi(_handle, (int)xInd.Size, alpha.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSaxpyi", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Addition of a scalar multiple of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Axpyi(CudaDeviceVariable<double> alpha, CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<double> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDaxpyi(_handle, (int)xInd.Size, alpha.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDaxpyi", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Addition of a scalar multiple of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Axpyi(CudaDeviceVariable<cuFloatComplex> alpha, CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuFloatComplex> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCaxpyi(_handle, (int)xInd.Size, alpha.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCaxpyi", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Addition of a scalar multiple of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Axpyi(CudaDeviceVariable<cuDoubleComplex> alpha, CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuDoubleComplex> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZaxpyi(_handle, (int)xInd.Size, alpha.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZaxpyi", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// dot product of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Doti(CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<float> y, ref float result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSdoti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, ref result, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSdoti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// dot product of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Doti(CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<double> y, ref double result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDdoti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, ref result, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDdoti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// dot product of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Doti(CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuFloatComplex> y, ref cuFloatComplex result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCdoti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, ref result, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCdoti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// dot product of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Doti(CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuDoubleComplex> y, ref cuDoubleComplex result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZdoti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, ref result, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZdoti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// dot product of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Doti(CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<float> y, CudaDeviceVariable<float> result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSdoti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, result.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSdoti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// dot product of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Doti(CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<double> y, CudaDeviceVariable<double> result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDdoti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, result.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDdoti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// dot product of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Doti(CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuFloatComplex> y, CudaDeviceVariable<cuFloatComplex> result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCdoti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, result.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCdoti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// dot product of a sparse vector x and a dense vector y
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Doti(CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuDoubleComplex> y, CudaDeviceVariable<cuDoubleComplex> result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZdoti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, result.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZdoti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// dot product of complex conjugate of a sparse vector x and a dense vector y.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Dotci(CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuFloatComplex> y, ref cuFloatComplex result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCdotci(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, ref result, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCdotci", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// dot product of complex conjugate of a sparse vector x and a dense vector y.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Dotci(CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuDoubleComplex> y, ref cuDoubleComplex result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZdotci(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, ref result, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZdotci", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// dot product of complex conjugate of a sparse vector x and a dense vector y.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Dotci(CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuFloatComplex> y, CudaDeviceVariable<cuFloatComplex> result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCdotci(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, result.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCdotci", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// dot product of complex conjugate of a sparse vector x and a dense vector y.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="result">pointer to the location of the result in the device or host memory.</param>
		/// <param name="idxBase">Index base.</param>
		public void Dotci(CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuDoubleComplex> y, CudaDeviceVariable<cuDoubleComplex> result, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZdotci(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, result.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZdotci", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// Gather of non-zero elements from dense vector y into sparse vector x.
		/// </summary>
		/// <param name="y">vector in dense format (of size >= max(xInd)-idxBase+1).</param>
		/// <param name="xVal">vector with nnz non-zero values that were gathered from vector y (that is unchanged if nnz == 0).</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Gthr(CudaDeviceVariable<float> y, CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSgthr(_handle, (int)xInd.Size, y.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgthr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Gather of non-zero elements from dense vector y into sparse vector x.
		/// </summary>
		/// <param name="y">vector in dense format (of size >= max(xInd)-idxBase+1).</param>
		/// <param name="xVal">vector with nnz non-zero values that were gathered from vector y (that is unchanged if nnz == 0).</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Gthr(CudaDeviceVariable<double> y, CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDgthr(_handle, (int)xInd.Size, y.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgthr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Gather of non-zero elements from dense vector y into sparse vector x.
		/// </summary>
		/// <param name="y">vector in dense format (of size >= max(xInd)-idxBase+1).</param>
		/// <param name="xVal">vector with nnz non-zero values that were gathered from vector y (that is unchanged if nnz == 0).</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Gthr(CudaDeviceVariable<cuFloatComplex> y, CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCgthr(_handle, (int)xInd.Size, y.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgthr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Gather of non-zero elements from dense vector y into sparse vector x.
		/// </summary>
		/// <param name="y">vector in dense format (of size >= max(xInd)-idxBase+1).</param>
		/// <param name="xVal">vector with nnz non-zero values that were gathered from vector y (that is unchanged if nnz == 0).</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Gthr(CudaDeviceVariable<cuDoubleComplex> y, CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZgthr(_handle, (int)xInd.Size, y.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgthr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// Gather of non-zero elements from desne vector y into sparse vector x (also replacing these elements in y by zeros).
		/// </summary>
		/// <param name="y">vector in dense format with elements indexed by xInd set to zero (it is unchanged if nnz == 0).</param>
		/// <param name="xVal">vector with nnz non-zero values that were gathered from vector y (that is unchanged if nnz == 0).</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Gthrz(CudaDeviceVariable<float> y, CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSgthrz(_handle, (int)xInd.Size, y.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgthrz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Gather of non-zero elements from desne vector y into sparse vector x (also replacing these elements in y by zeros).
		/// </summary>
		/// <param name="y">vector in dense format with elements indexed by xInd set to zero (it is unchanged if nnz == 0).</param>
		/// <param name="xVal">vector with nnz non-zero values that were gathered from vector y (that is unchanged if nnz == 0).</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Gthrz(CudaDeviceVariable<double> y, CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDgthrz(_handle, (int)xInd.Size, y.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgthrz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Gather of non-zero elements from desne vector y into sparse vector x (also replacing these elements in y by zeros).
		/// </summary>
		/// <param name="y">vector in dense format with elements indexed by xInd set to zero (it is unchanged if nnz == 0).</param>
		/// <param name="xVal">vector with nnz non-zero values that were gathered from vector y (that is unchanged if nnz == 0).</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Gthrz(CudaDeviceVariable<cuFloatComplex> y, CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCgthrz(_handle, (int)xInd.Size, y.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgthrz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Gather of non-zero elements from desne vector y into sparse vector x (also replacing these elements in y by zeros).
		/// </summary>
		/// <param name="y">vector in dense format with elements indexed by xInd set to zero (it is unchanged if nnz == 0).</param>
		/// <param name="xVal">vector with nnz non-zero values that were gathered from vector y (that is unchanged if nnz == 0).</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Gthrz(CudaDeviceVariable<cuDoubleComplex> y, CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZgthrz(_handle, (int)xInd.Size, y.DevicePointer, xVal.DevicePointer, xInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgthrz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// Scatter of elements of the sparse vector x into dense vector y.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Sctr(CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<float> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSsctr(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSsctr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Scatter of elements of the sparse vector x into dense vector y.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Sctr(CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<double> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDsctr(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDsctr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Scatter of elements of the sparse vector x into dense vector y.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Sctr(CudaDeviceVariable<cuFloatComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuFloatComplex> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCsctr(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCsctr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Scatter of elements of the sparse vector x into dense vector y.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="idxBase">Index base.</param>
		public void Sctr(CudaDeviceVariable<cuDoubleComplex> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<cuDoubleComplex> y, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZsctr(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZsctr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// Givens rotation, where c and s are cosine and sine, x and y are sparse and dense vectors, respectively.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="c">cosine element of the rotation matrix.</param>
		/// <param name="s">sine element of the rotation matrix.</param>
		/// <param name="idxBase">Index base.</param>
		public void Roti(CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<float> y, float c, float s, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSroti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, ref c, ref s, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSroti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Givens rotation, where c and s are cosine and sine, x and y are sparse and dense vectors, respectively.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="c">cosine element of the rotation matrix.</param>
		/// <param name="s">sine element of the rotation matrix.</param>
		/// <param name="idxBase">Index base.</param>
		public void Roti(CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<double> y, double c, double s, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDroti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, ref c, ref s, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDroti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Givens rotation, where c and s are cosine and sine, x and y are sparse and dense vectors, respectively.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="c">cosine element of the rotation matrix.</param>
		/// <param name="s">sine element of the rotation matrix.</param>
		/// <param name="idxBase">Index base.</param>
		public void Roti(CudaDeviceVariable<float> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<float> y, CudaDeviceVariable<float> c, CudaDeviceVariable<float> s, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseSroti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, c.DevicePointer, s.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSroti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Givens rotation, where c and s are cosine and sine, x and y are sparse and dense vectors, respectively.
		/// </summary>
		/// <param name="xVal">vector with nnz non-zero values of vector x.</param>
		/// <param name="xInd">integer vector with nnz indices of the non-zero values of vector x. Length of xInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="y">vector in dense format.</param>
		/// <param name="c">cosine element of the rotation matrix.</param>
		/// <param name="s">sine element of the rotation matrix.</param>
		/// <param name="idxBase">Index base.</param>
		public void Roti(CudaDeviceVariable<double> xVal, CudaDeviceVariable<int> xInd, CudaDeviceVariable<double> y, CudaDeviceVariable<double> c, CudaDeviceVariable<double> s, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDroti(_handle, (int)xInd.Size, xVal.DevicePointer, xInd.DevicePointer, y.DevicePointer, c.DevicePointer, s.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDroti", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		#endregion

		#region Sparse Level 2 routines
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC,
		/// and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. 
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="x">vector of n elements if op(A) = A, and m elements if op(A) =
		/// AT or op(A) = AH.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements if op(A) = A and n elements if op(A) = AT or op(A) = AH.</param>
		public void Csrmv(cusparseOperation transA, int m, int n, float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<float> x, float beta, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseScsrmv(_handle, transA, m, n, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC,
		/// and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. 
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="x">vector of n elements if op(A) = A, and m elements if op(A) =
		/// AT or op(A) = AH.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements if op(A) = A and n elements if op(A) = AT or op(A) = AH.</param>
		public void Csrmv(cusparseOperation transA, int m, int n, double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<double> x, double beta, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDcsrmv(_handle, transA, m, n, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC,
		/// and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. 
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="x">vector of n elements if op(A) = A, and m elements if op(A) =
		/// AT or op(A) = AH.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements if op(A) = A and n elements if op(A) = AT or op(A) = AH.</param>
		public void Csrmv(cusparseOperation transA, int m, int n, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuFloatComplex> x, cuFloatComplex beta, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCcsrmv(_handle, transA, m, n, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC,
		/// and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. 
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="x">vector of n elements if op(A) = A, and m elements if op(A) =
		/// AT or op(A) = AH.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements if op(A) = A and n elements if op(A) = AT or op(A) = AH.</param>
		public void Csrmv(cusparseOperation transA, int m, int n, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuDoubleComplex> x, cuDoubleComplex beta, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZcsrmv(_handle, transA, m, n, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC,
		/// and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. 
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="x">vector of n elements if op(A) = A, and m elements if op(A) =
		/// AT or op(A) = AH.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements if op(A) = A and n elements if op(A) = AT or op(A) = AH.</param>
		public void Csrmv(cusparseOperation transA, int m, int n, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<float> x, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseScsrmv(_handle, transA, m, n, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC,
		/// and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. 
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="x">vector of n elements if op(A) = A, and m elements if op(A) =
		/// AT or op(A) = AH.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements if op(A) = A and n elements if op(A) = AT or op(A) = AH.</param>
		public void Csrmv(cusparseOperation transA, int m, int n, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<double> x, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDcsrmv(_handle, transA, m, n, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC,
		/// and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. 
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="x">vector of n elements if op(A) = A, and m elements if op(A) =
		/// AT or op(A) = AH.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements if op(A) = A and n elements if op(A) = AT or op(A) = AH.</param>
		public void Csrmv(cusparseOperation transA, int m, int n, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuFloatComplex> x, CudaDeviceVariable<cuFloatComplex> beta, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCcsrmv(_handle, transA, m, n, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC,
		/// and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. 
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="x">vector of n elements if op(A) = A, and m elements if op(A) =
		/// AT or op(A) = AH.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements if op(A) = A and n elements if op(A) = AT or op(A) = AH.</param>
		public void Csrmv(cusparseOperation transA, int m, int n, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuDoubleComplex> x, CudaDeviceVariable<cuDoubleComplex> beta, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZcsrmv(_handle, transA, m, n, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="x">vector of n elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements.</param>
		public void Hybmv(cusparseOperation transA, float alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<float> x, float beta, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseShybmv(_handle, transA, ref alpha, descrA.Descriptor, hybA.HybMat, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseShybmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="x">vector of n elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements.</param>
		public void Hybmv(cusparseOperation transA, double alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<double> x, double beta, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDhybmv(_handle, transA, ref alpha, descrA.Descriptor, hybA.HybMat, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDhybmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="x">vector of n elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements.</param>
		public void Hybmv(cusparseOperation transA, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuFloatComplex> x, cuFloatComplex beta, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseChybmv(_handle, transA, ref alpha, descrA.Descriptor, hybA.HybMat, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseChybmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="x">vector of n elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements.</param>
		public void Hybmv(cusparseOperation transA, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuDoubleComplex> x, cuDoubleComplex beta, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZhybmv(_handle, transA, ref alpha, descrA.Descriptor, hybA.HybMat, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZhybmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="x">vector of n elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements.</param>
		public void Hybmv(cusparseOperation transA, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<float> x, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseShybmv(_handle, transA, alpha.DevicePointer, descrA.Descriptor, hybA.HybMat, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseShybmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="x">vector of n elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements.</param>
		public void Hybmv(cusparseOperation transA, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<double> x, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDhybmv(_handle, transA, alpha.DevicePointer, descrA.Descriptor, hybA.HybMat, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDhybmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="x">vector of n elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements.</param>
		public void Hybmv(cusparseOperation transA, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuFloatComplex> x, CudaDeviceVariable<cuFloatComplex> beta, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseChybmv(_handle, transA, alpha.DevicePointer, descrA.Descriptor, hybA.HybMat, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseChybmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-vector multiplication  y = alpha * op(A) * x  + beta * y, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="x">vector of n elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of m elements.</param>
		public void Hybmv(cusparseOperation transA, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuDoubleComplex> x, CudaDeviceVariable<cuDoubleComplex> beta, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZhybmv(_handle, transA, alpha.DevicePointer, descrA.Descriptor, hybA.HybMat, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZhybmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		public void CsrsvAnalysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseScsrsv_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsv_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		public void CsrsvAnalysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsv_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsv_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		public void CsrsvAnalysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsv_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsv_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		public void CsrsvAnalysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsv_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsv_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void CsrsvSolve(cusparseOperation transA, int m, float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<float> x, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseScsrsv_solve(_handle, transA, m, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void CsrsvSolve(cusparseOperation transA, int m, double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<double> x, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsv_solve(_handle, transA, m, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void CsrsvSolve(cusparseOperation transA, int m, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuFloatComplex> x, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsv_solve(_handle, transA, m, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void CsrsvSolve(cusparseOperation transA, int m, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuDoubleComplex> x, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsv_solve(_handle, transA, m, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void CsrsvSolve(cusparseOperation transA, int m, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<float> x, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseScsrsv_solve(_handle, transA, m, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void CsrsvSolve(cusparseOperation transA, int m, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<double> x, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsv_solve(_handle, transA, m, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void CsrsvSolve(cusparseOperation transA, int m, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuFloatComplex> x, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsv_solve(_handle, transA, m, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in CSR storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void CsrsvSolve(cusparseOperation transA, int m, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuDoubleComplex> x, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsv_solve(_handle, transA, m, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}




		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <typeparam name="T">data type: float, double, cuFloatComplex or cuDoubleComplex</typeparam>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type 
		/// CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		public void HybsvAnalysis<T>(cusparseOperation transA, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info) where T : struct
		{
			Type t = typeof(T);
			if (t == typeof(float))
			{
				res = CudaSparseNativeMethods.cusparseShybsv_analysis(_handle, transA, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo);
				Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseShybsv_analysis", res));
			}
			else if (t == typeof(double))
			{
				res = CudaSparseNativeMethods.cusparseDhybsv_analysis(_handle, transA, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo);
				Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDhybsv_analysis", res));
			}
			else if (t == typeof(cuFloatComplex))
			{
				res = CudaSparseNativeMethods.cusparseChybsv_analysis(_handle, transA, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo);
				Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseChybsv_analysis", res));
			}
			else if (t == typeof(cuDoubleComplex))
			{
				res = CudaSparseNativeMethods.cusparseZhybsv_analysis(_handle, transA, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo);
				Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZhybsv_analysis", res));
			}
			else
				throw new CudaSparseException("The type '" + t.Name + "' is not supported in HybsvAnalysis.");

			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void HybsvSolve(cusparseOperation transA, float alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<float> x, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseShybsv_solve(_handle, transA, ref alpha, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseShybsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void HybsvSolve(cusparseOperation transA, double alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<double> x, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDhybsv_solve(_handle, transA, ref alpha, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDhybsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void HybsvSolve(cusparseOperation transA, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuFloatComplex> x, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseChybsv_solve(_handle, transA, ref alpha, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseChybsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void HybsvSolve(cusparseOperation transA, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuDoubleComplex> x, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZhybsv_solve(_handle, transA, ref alpha, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZhybsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void HybsvSolve(cusparseOperation transA, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<float> x, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseShybsv_solve(_handle, transA, alpha.DevicePointer, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseShybsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void HybsvSolve(cusparseOperation transA, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<double> x, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDhybsv_solve(_handle, transA, alpha.DevicePointer, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDhybsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void HybsvSolve(cusparseOperation transA, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuFloatComplex> x, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseChybsv_solve(_handle, transA, alpha.DevicePointer, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseChybsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * y = alpha * x, 
		/// where A is a sparse matrix in HYB storage format, x and y are dense vectors.
		/// </summary>
		/// <param name="transA">the operation op(A) (currently only op(A) = A is supported).</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal type CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="info">structure filled with information collected during the analysis phase
		/// (that should be passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void HybsvSolve(cusparseOperation transA, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuDoubleComplex> x, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZhybsv_solve(_handle, transA, alpha.DevicePointer, descrA.Descriptor, hybA.HybMat, info.SolveAnalysisInfo, x.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZhybsv_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXcsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called csrsv2_analysis() or csrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Csrsv2ZeroPivot(CudaSparseCsrsv2Info info, CudaDeviceVariable<int> position)
		{
			res = CudaSparseNativeMethods.cusparseXcsrsv2_zeroPivot(_handle, info.Csrsv2Info, position.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrsv2_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}
		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXcsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called csrsv2_analysis() or csrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Csrsv2ZeroPivot(CudaSparseCsrsv2Info info, ref int position)
		{
			res = CudaSparseNativeMethods.cusparseXcsrsv2_zeroPivot(_handle, info.Csrsv2Info, ref position);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrsv2_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}

		/// <summary>
		/// This function returns the size of the buffer used in csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csrsv2BufferSize(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseScsrsv2_bufferSizeExt(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsv2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns the size of the buffer used in csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csrsv2BufferSize(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDcsrsv2_bufferSizeExt(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsv2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns the size of the buffer used in csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csrsv2BufferSize(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCcsrsv2_bufferSizeExt(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsv2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns the size of the buffer used in csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csrsv2BufferSize(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZcsrsv2_bufferSizeExt(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsv2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		

		/// <summary>
		/// This function performs the analysis phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrsv2Analysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseScsrsv2_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsv2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrsv2Analysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsv2_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsv2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrsv2Analysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsv2_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsv2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrsv2Analysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsv2_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsv2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		

		/// <summary>
		/// This function performs the solve phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Csrsv2Solve(cusparseOperation transA, int m, ref float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, 
			CudaDeviceVariable<float> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseScsrsv2_solve(_handle, transA, m, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Csrsv2Solve(cusparseOperation transA, int m, ref double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, 
			CudaDeviceVariable<double> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsv2_solve(_handle, transA, m, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Csrsv2Solve(cusparseOperation transA, int m, ref cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, 
			CudaDeviceVariable<cuFloatComplex> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsv2_solve(_handle, transA, m, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Csrsv2Solve(cusparseOperation transA, int m, ref cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, 
			CudaDeviceVariable<cuDoubleComplex> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsv2_solve(_handle, transA, m, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Csrsv2Solve(cusparseOperation transA, int m, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, 
			CudaDeviceVariable<float> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseScsrsv2_solve(_handle, transA, m, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Csrsv2Solve(cusparseOperation transA, int m, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, 
			CudaDeviceVariable<double> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsv2_solve(_handle, transA, m, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Csrsv2Solve(cusparseOperation transA, int m, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, 
			CudaDeviceVariable<cuFloatComplex> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsv2_solve(_handle, transA, m, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of csrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Csrsv2Solve(cusparseOperation transA, int m, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrsv2Info info, 
			CudaDeviceVariable<cuDoubleComplex> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsv2_solve(_handle, transA, m, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}





		



		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXbsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called bsrsv2_analysis() or bsrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Bsrsv2ZeroPivot(CudaSparseBsrsv2Info info, CudaDeviceVariable<int> position)
		{
			res = CudaSparseNativeMethods.cusparseXbsrsv2_zeroPivot(_handle, info.Bsrsv2Info, position.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXbsrsv2_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}


		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXbsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called bsrsv2_analysis() or bsrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Bsrsv2ZeroPivot(CudaSparseBsrsv2Info info, ref int position)
		{
			res = CudaSparseNativeMethods.cusparseXbsrsv2_zeroPivot(_handle, info.Bsrsv2Info, ref position);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXbsrsv2_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}

		/// <summary>
		/// This function returns the size of the buffer used in bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		public SizeT Bsrsv2BufferSize(cusparseOperation transA, cusparseDirection dirA, int mb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseSbsrsv2_bufferSizeExt(_handle, dirA, transA, mb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrsv2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns the size of the buffer used in bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		public SizeT Bsrsv2BufferSize(cusparseOperation transA, cusparseDirection dirA, int mb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDbsrsv2_bufferSizeExt(_handle, dirA, transA, mb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrsv2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns the size of the buffer used in bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		public SizeT Bsrsv2BufferSize(cusparseOperation transA, cusparseDirection dirA, int mb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCbsrsv2_bufferSizeExt(_handle, dirA, transA, mb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrsv2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns the size of the buffer used in bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		public SizeT Bsrsv2BufferSize(cusparseOperation transA, cusparseDirection dirA, int mb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZbsrsv2_bufferSizeExt(_handle, dirA, transA, mb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrsv2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		

		/// <summary>
		/// This function performs the analysis phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		public void Bsrsv2Analysis(cusparseOperation transA, cusparseDirection dirA , int mb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseSbsrsv2_analysis(_handle, dirA, transA, mb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrsv2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		public void Bsrsv2Analysis(cusparseOperation transA, cusparseDirection dirA , int mb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDbsrsv2_analysis(_handle, dirA, transA, mb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrsv2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		public void Bsrsv2Analysis(cusparseOperation transA, cusparseDirection dirA , int mb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCbsrsv2_analysis(_handle, dirA, transA, mb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrsv2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		public void Bsrsv2Analysis(cusparseOperation transA, cusparseDirection dirA, int mb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZbsrsv2_analysis(_handle, dirA, transA, mb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrsv2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		

		/// <summary>
		/// This function performs the solve phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Bsrsv2Solve(cusparseOperation transA, cusparseDirection dirA , int mb, ref float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, 
			CudaDeviceVariable<float> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseSbsrsv2_solve(_handle, dirA, transA, mb, (int)bsrColIndA.Size, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Bsrsv2Solve(cusparseOperation transA, cusparseDirection dirA , int mb, ref double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, 
			CudaDeviceVariable<double> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDbsrsv2_solve(_handle, dirA, transA, mb, (int)bsrColIndA.Size, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Bsrsv2Solve(cusparseOperation transA, cusparseDirection dirA , int mb, ref cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, 
			CudaDeviceVariable<cuFloatComplex> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCbsrsv2_solve(_handle, dirA, transA, mb, (int)bsrColIndA.Size, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Bsrsv2Solve(cusparseOperation transA, cusparseDirection dirA , int mb, ref cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, 
			CudaDeviceVariable<cuDoubleComplex> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZbsrsv2_solve(_handle, dirA, transA, mb, (int)bsrColIndA.Size, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Bsrsv2Solve(cusparseOperation transA, cusparseDirection dirA , int mb, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, 
			CudaDeviceVariable<float> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseSbsrsv2_solve(_handle, dirA, transA, mb, (int)bsrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Bsrsv2Solve(cusparseOperation transA, cusparseDirection dirA , int mb, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, 
			CudaDeviceVariable<double> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDbsrsv2_solve(_handle, dirA, transA, mb, (int)bsrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Bsrsv2Solve(cusparseOperation transA, cusparseDirection dirA , int mb, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, 
			CudaDeviceVariable<cuFloatComplex> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCbsrsv2_solve(_handle, dirA, transA, mb, (int)bsrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of bsrsv2, a new sparse triangular
		/// linear system op(A)*y = x.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtrA(mb) - bsrRowPtrA(0)) column indices of the nonzero blocks of matrix A. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="blockDim">block dimension of sparse matrix A; must be larger than zero.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="x">right-hand-side vector of size m.</param>
		/// <param name="y">solution vector of size m.</param>
		public void Bsrsv2Solve(cusparseOperation transA, cusparseDirection dirA , int mb, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrsv2Info info, 
			CudaDeviceVariable<cuDoubleComplex> x, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZbsrsv2_solve(_handle, dirA, transA, mb, (int)bsrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrsv2Info, x.DevicePointer, y.DevicePointer, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrsv2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}












		#endregion

		#region Sparse Level 3 routines
		/// <summary>
		/// Matrix-matrix multiplication C = alpha * op(A) * B  + beta * C, where A is a sparse matrix, B and C are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrices B and C.</param>
		/// <param name="k">number of columns of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types 
		/// are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC, and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the
		/// supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="B">array of dimensions (ldb, n).</param>
		/// <param name="ldb">leading dimension of B. It must be at least max (1, k) if op(A) = A, and at least max (1, m) otherwise.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max (1, m) if op(A) = A and at least max (1, k) otherwise.</param>
		public void Csrmm(cusparseOperation transA, int m, int n, int k, float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<float> B, int ldb, float beta, CudaDeviceVariable<float> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseScsrmm(_handle, transA, m, n, k, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-matrix multiplication C = alpha * op(A) * B  + beta * C, where A is a sparse matrix, B and C are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrices B and C.</param>
		/// <param name="k">number of columns of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types 
		/// are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC, and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the
		/// supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="B">array of dimensions (ldb, n).</param>
		/// <param name="ldb">leading dimension of B. It must be at least max (1, k) if op(A) = A, and at least max (1, m) otherwise.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max (1, m) if op(A) = A and at least max (1, k) otherwise.</param>
		public void Csrmm(cusparseOperation transA, int m, int n, int k, double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<double> B, int ldb, double beta, CudaDeviceVariable<double> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseDcsrmm(_handle, transA, m, n, k, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-matrix multiplication C = alpha * op(A) * B  + beta * C, where A is a sparse matrix, B and C are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrices B and C.</param>
		/// <param name="k">number of columns of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types 
		/// are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC, and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the
		/// supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="B">array of dimensions (ldb, n).</param>
		/// <param name="ldb">leading dimension of B. It must be at least max (1, k) if op(A) = A, and at least max (1, m) otherwise.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max (1, m) if op(A) = A and at least max (1, k) otherwise.</param>
		public void Csrmm(cusparseOperation transA, int m, int n, int k, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuFloatComplex> B, int ldb, cuFloatComplex beta, CudaDeviceVariable<cuFloatComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseCcsrmm(_handle, transA, m, n, k, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-matrix multiplication C = alpha * op(A) * B  + beta * C, where A is a sparse matrix, B and C are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrices B and C.</param>
		/// <param name="k">number of columns of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types 
		/// are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC, and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the
		/// supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="B">array of dimensions (ldb, n).</param>
		/// <param name="ldb">leading dimension of B. It must be at least max (1, k) if op(A) = A, and at least max (1, m) otherwise.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max (1, m) if op(A) = A and at least max (1, k) otherwise.</param>
		public void Csrmm(cusparseOperation transA, int m, int n, int k, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuDoubleComplex> B, int ldb, cuDoubleComplex beta, CudaDeviceVariable<cuDoubleComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseZcsrmm(_handle, transA, m, n, k, (int)csrColIndA.Size, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// Matrix-matrix multiplication C = alpha * op(A) * B  + beta * C, where A is a sparse matrix, B and C are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrices B and C.</param>
		/// <param name="k">number of columns of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types 
		/// are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC, and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the
		/// supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="B">array of dimensions (ldb, n).</param>
		/// <param name="ldb">leading dimension of B. It must be at least max (1, k) if op(A) = A, and at least max (1, m) otherwise.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max (1, m) if op(A) = A and at least max (1, k) otherwise.</param>
		public void Csrmm(cusparseOperation transA, int m, int n, int k, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<float> B, int ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseScsrmm(_handle, transA, m, n, k, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-matrix multiplication C = alpha * op(A) * B  + beta * C, where A is a sparse matrix, B and C are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrices B and C.</param>
		/// <param name="k">number of columns of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types 
		/// are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC, and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the
		/// supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="B">array of dimensions (ldb, n).</param>
		/// <param name="ldb">leading dimension of B. It must be at least max (1, k) if op(A) = A, and at least max (1, m) otherwise.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max (1, m) if op(A) = A and at least max (1, k) otherwise.</param>
		public void Csrmm(cusparseOperation transA, int m, int n, int k, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<double> B, int ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseDcsrmm(_handle, transA, m, n, k, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-matrix multiplication C = alpha * op(A) * B  + beta * C, where A is a sparse matrix, B and C are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrices B and C.</param>
		/// <param name="k">number of columns of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types 
		/// are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC, and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the
		/// supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="B">array of dimensions (ldb, n).</param>
		/// <param name="ldb">leading dimension of B. It must be at least max (1, k) if op(A) = A, and at least max (1, m) otherwise.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max (1, m) if op(A) = A and at least max (1, k) otherwise.</param>
		public void Csrmm(cusparseOperation transA, int m, int n, int k, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuFloatComplex> B, int ldb, CudaDeviceVariable<cuFloatComplex> beta, CudaDeviceVariable<cuFloatComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseCcsrmm(_handle, transA, m, n, k, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Matrix-matrix multiplication C = alpha * op(A) * B  + beta * C, where A is a sparse matrix, B and C are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrices B and C.</param>
		/// <param name="k">number of columns of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix types 
		/// are CUSPARSE_MATRIX_TYPE_GENERAL, CUSPARSE_MATRIX_TYPE_SYMMETRIC, and CUSPARSE_MATRIX_TYPE_HERMITIAN. Also, the
		/// supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="B">array of dimensions (ldb, n).</param>
		/// <param name="ldb">leading dimension of B. It must be at least max (1, k) if op(A) = A, and at least max (1, m) otherwise.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max (1, m) if op(A) = A and at least max (1, k) otherwise.</param>
		public void Csrmm(cusparseOperation transA, int m, int n, int k, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuDoubleComplex> B, int ldb, CudaDeviceVariable<cuDoubleComplex> beta, CudaDeviceVariable<cuDoubleComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseZcsrmm(_handle, transA, m, n, k, (int)csrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		#region ref host
		/// <summary/>
		public void Csrmm2(cusparseOperation transa, cusparseOperation transb, int m, int n, int k, int nnz,
							ref float alpha, cusparseMatDescr descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, 
							CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<float> B, int ldb, ref float beta, CudaDeviceVariable<float> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseScsrmm2(_handle, transa, transb, m, n, k, nnz, ref alpha, descrA, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrmm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary/>
		public void Csrmm2(cusparseOperation transa, cusparseOperation transb, int m, int n, int k, int nnz,
							ref double alpha, cusparseMatDescr descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA,
							CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<double> B, int ldb, ref double beta, CudaDeviceVariable<double> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseDcsrmm2(_handle, transa, transb, m, n, k, nnz, ref alpha, descrA, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrmm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary/>
		public void Csrmm2(cusparseOperation transa, cusparseOperation transb, int m, int n, int k, int nnz,
							ref cuFloatComplex alpha, cusparseMatDescr descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA,
							CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuFloatComplex> B, int ldb, ref cuFloatComplex beta, CudaDeviceVariable<cuFloatComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseCcsrmm2(_handle, transa, transb, m, n, k, nnz, ref alpha, descrA, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrmm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary/>
		public void Csrmm2(cusparseOperation transa, cusparseOperation transb, int m, int n, int k, int nnz,
							ref cuDoubleComplex alpha, cusparseMatDescr descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA,
							CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuDoubleComplex> B, int ldb, ref cuDoubleComplex beta, CudaDeviceVariable<cuDoubleComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseZcsrmm2(_handle, transa, transb, m, n, k, nnz, ref alpha, descrA, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrmm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		#endregion

		#region ref device
		/// <summary/>
		public void Csrmm2(cusparseOperation transa, cusparseOperation transb, int m, int n, int k, int nnz,
							CudaDeviceVariable<float> alpha, cusparseMatDescr descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA,
							CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<float> B, int ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseScsrmm2(_handle, transa, transb, m, n, k, nnz, alpha.DevicePointer, descrA, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrmm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary/>
		public void Csrmm2(cusparseOperation transa, cusparseOperation transb, int m, int n, int k, int nnz,
							CudaDeviceVariable<double> alpha, cusparseMatDescr descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA,
							CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<double> B, int ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseDcsrmm2(_handle, transa, transb, m, n, k, nnz, alpha.DevicePointer, descrA, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrmm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary/>
		public void Csrmm2(cusparseOperation transa, cusparseOperation transb, int m, int n, int k, int nnz,
							CudaDeviceVariable<cuFloatComplex> alpha, cusparseMatDescr descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA,
							CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuFloatComplex> B, int ldb, CudaDeviceVariable<cuFloatComplex> beta, CudaDeviceVariable<cuFloatComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseCcsrmm2(_handle, transa, transb, m, n, k, nnz, alpha.DevicePointer, descrA, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrmm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary/>
		public void Csrmm2(cusparseOperation transa, cusparseOperation transb, int m, int n, int k, int nnz,
							CudaDeviceVariable<cuDoubleComplex> alpha, cusparseMatDescr descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA,
							CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuDoubleComplex> B, int ldb, CudaDeviceVariable<cuDoubleComplex> beta, CudaDeviceVariable<cuDoubleComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseZcsrmm2(_handle, transa, transb, m, n, k, nnz, alpha.DevicePointer, descrA, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrmm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		#endregion


		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		public void CsrsmAnalysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseScsrsm_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsm_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		public void CsrsmAnalysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsm_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsm_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		public void CsrsmAnalysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsm_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsm_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure filled with information collected during the analysis phase (that should be passed to the solve phase unchanged).</param>
		public void CsrsmAnalysis(cusparseOperation transA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsm_analysis(_handle, transA, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsm_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="n">number of columns of matrix X and Y .</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure with information collected during the analysis phase (that should have been passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side array of dimensions (ldx, n).</param>
		/// <param name="ldx">leading dimension of X (that is >= max(1;m)).</param>
		/// <param name="y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y (that is >= max(1;m)).</param>
		public void CsrsmSolve(cusparseOperation transA, int m, int n, float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<float> x, int ldx, CudaDeviceVariable<float> y, int ldy)
		{
			res = CudaSparseNativeMethods.cusparseScsrsm_solve(_handle, transA, m, n, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, ldx, y.DevicePointer, ldy);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsm_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="n">number of columns of matrix X and Y .</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure with information collected during the analysis phase (that should have been passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side array of dimensions (ldx, n).</param>
		/// <param name="ldx">leading dimension of X (that is >= max(1;m)).</param>
		/// <param name="y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y (that is >= max(1;m)).</param>
		public void CsrsmSolve(cusparseOperation transA, int m, int n, double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<double> x, int ldx, CudaDeviceVariable<double> y, int ldy)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsm_solve(_handle, transA, m, n, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, ldx, y.DevicePointer, ldy);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsm_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="n">number of columns of matrix X and Y .</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure with information collected during the analysis phase (that should have been passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side array of dimensions (ldx, n).</param>
		/// <param name="ldx">leading dimension of X (that is >= max(1;m)).</param>
		/// <param name="y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y (that is >= max(1;m)).</param>
		public void CsrsmSolve(cusparseOperation transA, int m, int n, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuFloatComplex> x, int ldx, CudaDeviceVariable<cuFloatComplex> y, int ldy)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsm_solve(_handle, transA, m, n, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, ldx, y.DevicePointer, ldy);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsm_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="n">number of columns of matrix X and Y .</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure with information collected during the analysis phase (that should have been passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side array of dimensions (ldx, n).</param>
		/// <param name="ldx">leading dimension of X (that is >= max(1;m)).</param>
		/// <param name="y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y (that is >= max(1;m)).</param>
		public void CsrsmSolve(cusparseOperation transA, int m, int n, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuDoubleComplex> x, int ldx, CudaDeviceVariable<cuDoubleComplex> y, int ldy)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsm_solve(_handle, transA, m, n, ref alpha, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, ldx, y.DevicePointer, ldy);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsm_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="n">number of columns of matrix X and Y .</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure with information collected during the analysis phase (that should have been passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side array of dimensions (ldx, n).</param>
		/// <param name="ldx">leading dimension of X (that is >= max(1;m)).</param>
		/// <param name="y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y (that is >= max(1;m)).</param>
		public void CsrsmSolve(cusparseOperation transA, int m, int n, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<float> x, int ldx, CudaDeviceVariable<float> y, int ldy)
		{
			res = CudaSparseNativeMethods.cusparseScsrsm_solve(_handle, transA, m, n, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, ldx, y.DevicePointer, ldy);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrsm_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="n">number of columns of matrix X and Y .</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure with information collected during the analysis phase (that should have been passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side array of dimensions (ldx, n).</param>
		/// <param name="ldx">leading dimension of X (that is >= max(1;m)).</param>
		/// <param name="y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y (that is >= max(1;m)).</param>
		public void CsrsmSolve(cusparseOperation transA, int m, int n, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<double> x, int ldx, CudaDeviceVariable<double> y, int ldy)
		{
			res = CudaSparseNativeMethods.cusparseDcsrsm_solve(_handle, transA, m, n, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, ldx, y.DevicePointer, ldy);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrsm_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="n">number of columns of matrix X and Y .</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure with information collected during the analysis phase (that should have been passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side array of dimensions (ldx, n).</param>
		/// <param name="ldx">leading dimension of X (that is >= max(1;m)).</param>
		/// <param name="y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y (that is >= max(1;m)).</param>
		public void CsrsmSolve(cusparseOperation transA, int m, int n, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuFloatComplex> x, int ldx, CudaDeviceVariable<cuFloatComplex> y, int ldy)
		{
			res = CudaSparseNativeMethods.cusparseCcsrsm_solve(_handle, transA, m, n, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, ldx, y.DevicePointer, ldy);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrsm_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of triangular linear system op(A) * Y = alpha * X, with multiple right-hand-sides, where A is a sparse matrix in CSR storage 
		/// format, X and Y are dense and usually tall matrices.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="n">number of columns of matrix X and Y .</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal types 
		/// CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="info">structure with information collected during the analysis phase (that should have been passed to the solve phase unchanged).</param>
		/// <param name="x">right-hand-side array of dimensions (ldx, n).</param>
		/// <param name="ldx">leading dimension of X (that is >= max(1;m)).</param>
		/// <param name="y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y (that is >= max(1;m)).</param>
		public void CsrsmSolve(cusparseOperation transA, int m, int n, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info, CudaDeviceVariable<cuDoubleComplex> x, int ldx, CudaDeviceVariable<cuDoubleComplex> y, int ldy)
		{
			res = CudaSparseNativeMethods.cusparseZcsrsm_solve(_handle, transA, m, n, alpha.DevicePointer, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo, x.DevicePointer, ldx, y.DevicePointer, ldy);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrsm_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) is either a structural zero or a numerical zero (singular block). Otherwise
		/// position=-1. <para/>
		/// The position can be 0-base or 1-base, the same as the matrix.
		/// Function cusparseXbsrsm2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done.<para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains a structural zero or a
		/// numerical zero if the user already called bsrsm2_analysis() or bsrsm2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1;
		/// otherwise, if A(j,j) is missing or U(j,j) is zero,
		/// position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Xbsrsm2ZeroPivot(CudaSparseBsrsm2Info info, ref int position)
		{
			res = CudaSparseNativeMethods.cusparseXbsrsm2_zeroPivot(_handle, info.Bsrsm2Info, ref position);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXbsrsm2_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}

		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) is either a structural zero or a numerical zero (singular block). Otherwise
		/// position=-1. <para/>
		/// The position can be 0-base or 1-base, the same as the matrix.
		/// Function cusparseXbsrsm2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done.<para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains a structural zero or a
		/// numerical zero if the user already called bsrsm2_analysis() or bsrsm2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1;
		/// otherwise, if A(j,j) is missing or U(j,j) is zero,
		/// position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Xbsrsm2ZeroPivot(CudaSparseBsrsm2Info info, CudaDeviceVariable<int> position)
		{
			res = CudaSparseNativeMethods.cusparseXbsrsm2_zeroPivot(_handle, info.Bsrsm2Info, position.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXbsrsm2_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}

		/// <summary>
		/// This function returns size of buffer used in bsrsm2(), a new sparse triangular linear
		/// system op(A)*Y = alpha op(X).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either 
		/// CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(X).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of matrix Y and op(X).</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL, while the supported diagonal types are CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrVal">array of nnzb bsrRowPtrA(mb) 
		/// bsrRowPtrA(0) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb +1 elements that contains the
		/// start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb (= bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A; larger than 
		/// zero.</param>
		/// <param name="info">record internal states based on different algorithms.</param>
		/// <returns>number of bytes of the buffer used in bsrsm2_analysis() and bsrsm2_solve().</returns>
		public SizeT Bsrsm2BufferSize(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY,
									int mb, int n, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrVal, CudaDeviceVariable<int> bsrRowPtr,
									CudaDeviceVariable<int> bsrColInd, int blockSize, CudaSparseBsrsm2Info info)
		{
			SizeT buffersize = 0;
			res = CudaSparseNativeMethods.cusparseSbsrsm2_bufferSizeExt(_handle, dirA, transA, transXY, mb, n, nnzb, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, ref buffersize);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrsm2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return buffersize;
		}

		/// <summary>
		/// This function returns size of buffer used in bsrsm2(), a new sparse triangular linear
		/// system op(A)*Y = alpha op(X).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either 
		/// CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(X).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of matrix Y and op(X).</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL, while the supported diagonal types are CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrVal">array of nnzb bsrRowPtrA(mb) 
		/// bsrRowPtrA(0) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb +1 elements that contains the
		/// start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb (= bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A; larger than 
		/// zero.</param>
		/// <param name="info">record internal states based on different algorithms.</param>
		/// <returns>number of bytes of the buffer used in bsrsm2_analysis() and bsrsm2_solve().</returns>
		public SizeT Bsrsm2BufferSize(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY,
									int mb, int n, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrVal, CudaDeviceVariable<int> bsrRowPtr,
									CudaDeviceVariable<int> bsrColInd, int blockSize, CudaSparseBsrsm2Info info)
		{
			SizeT buffersize = 0;
			res = CudaSparseNativeMethods.cusparseDbsrsm2_bufferSizeExt(_handle, dirA, transA, transXY, mb, n, nnzb, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, ref buffersize);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrsm2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return buffersize;
		}

		/// <summary>
		/// This function returns size of buffer used in bsrsm2(), a new sparse triangular linear
		/// system op(A)*Y = alpha op(X).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either 
		/// CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(X).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of matrix Y and op(X).</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL, while the supported diagonal types are CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrVal">array of nnzb bsrRowPtrA(mb) 
		/// bsrRowPtrA(0) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb +1 elements that contains the
		/// start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb (= bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A; larger than 
		/// zero.</param>
		/// <param name="info">record internal states based on different algorithms.</param>
		/// <returns>number of bytes of the buffer used in bsrsm2_analysis() and bsrsm2_solve().</returns>
		public SizeT Bsrsm2BufferSize(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY,
									int mb, int n, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr,
									CudaDeviceVariable<int> bsrColInd, int blockSize, CudaSparseBsrsm2Info info)
		{
			SizeT buffersize = 0;
			res = CudaSparseNativeMethods.cusparseCbsrsm2_bufferSizeExt(_handle, dirA, transA, transXY, mb, n, nnzb, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, ref buffersize);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrsm2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return buffersize;
		}

		/// <summary>
		/// This function returns size of buffer used in bsrsm2(), a new sparse triangular linear
		/// system op(A)*Y = alpha op(X).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either 
		/// CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(X).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of matrix Y and op(X).</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL, while the supported diagonal types are CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrVal">array of nnzb bsrRowPtrA(mb) 
		/// bsrRowPtrA(0) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb +1 elements that contains the
		/// start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb (= bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A; larger than 
		/// zero.</param>
		/// <param name="info">record internal states based on different algorithms.</param>
		/// <returns>number of bytes of the buffer used in bsrsm2_analysis() and bsrsm2_solve().</returns>
		public SizeT Bsrsm2BufferSize(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY,
									int mb, int n, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr,
									CudaDeviceVariable<int> bsrColInd, int blockSize, CudaSparseBsrsm2Info info)
		{
			SizeT buffersize = 0;
			res = CudaSparseNativeMethods.cusparseZbsrsm2_bufferSizeExt(_handle, dirA, transA, transXY, mb, n, nnzb, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, ref buffersize);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrsm2_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return buffersize;
		}
		

		/// <summary>
		/// This function performs the analysis phase of bsrsm2(), a new sparse triangular linear
		/// system op(A)*op(Y) = alpha op(X).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(X).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of matrix Y and op(X).</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL, while the supported diagonal types are CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrVal">array of nnzb bsrRowPtrA(mb) 
		/// bsrRowPtrA(0) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb +1 elements that contains the
		/// start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb (= bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A; larger than 
		/// zero.</param>
		/// <param name="info">record internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are 
		/// CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is return by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Analysis(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY,
									int mb, int n, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrVal,
									CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd, int blockSize,
									CudaSparseBsrsm2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseSbsrsm2_analysis(_handle, dirA, transA, transXY, mb, n, nnzb, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, 
				blockSize, info.Bsrsm2Info, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrsm2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the analysis phase of bsrsm2(), a new sparse triangular linear
		/// system op(A)*op(Y) = alpha op(X).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(X).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of matrix Y and op(X).</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL, while the supported diagonal types are CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrVal">array of nnzb bsrRowPtrA(mb) 
		/// bsrRowPtrA(0) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb +1 elements that contains the
		/// start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb (= bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A; larger than 
		/// zero.</param>
		/// <param name="info">record internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are 
		/// CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is return by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Analysis(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY,
									int mb, int n, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrVal,
									CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd, int blockSize,
									CudaSparseBsrsm2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseDbsrsm2_analysis(_handle, dirA, transA, transXY, mb, n, nnzb, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer,
				blockSize, info.Bsrsm2Info, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrsm2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of bsrsm2(), a new sparse triangular linear
		/// system op(A)*op(Y) = alpha op(X).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(X).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of matrix Y and op(X).</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL, while the supported diagonal types are CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrVal">array of nnzb bsrRowPtrA(mb) 
		/// bsrRowPtrA(0) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb +1 elements that contains the
		/// start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb (= bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A; larger than 
		/// zero.</param>
		/// <param name="info">record internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are 
		/// CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is return by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Analysis(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY,
									int mb, int n, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrVal,
									CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd, int blockSize,
									CudaSparseBsrsm2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseCbsrsm2_analysis(_handle, dirA, transA, transXY, mb, n, nnzb, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer,
				blockSize, info.Bsrsm2Info, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrsm2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of bsrsm2(), a new sparse triangular linear
		/// system op(A)*op(Y) = alpha op(X).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(X).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of matrix Y and op(X).</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL, while the supported diagonal types are CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrVal">array of nnzb bsrRowPtrA(mb) 
		/// bsrRowPtrA(0) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb +1 elements that contains the
		/// start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb (= bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A; larger than 
		/// zero.</param>
		/// <param name="info">record internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are 
		/// CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is return by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Analysis(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY,
									int mb, int n, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrVal,
									CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd, int blockSize,
									CudaSparseBsrsm2Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseZbsrsm2_analysis(_handle, dirA, transA, transXY, mb, n, nnzb, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer,
				blockSize, info.Bsrsm2Info, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrsm2_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}




		#region host
		/// <summary>
		/// This function performs one of the following matrix-matrix operations:
		/// C = alpha * op(A) * op(B) + beta * C
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrix op(B) and A.</param>
		/// <param name="kb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of non-zero blocks of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb + 1elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="B">array of dimensions (ldb, n) if op(B)=B and (ldb, k) otherwise.</param>
		/// <param name="ldb">leading dimension of B. If op(B)=B, it must be at least max(l,k) If op(B) != B, it must be at least
		/// max(1, n).</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max(l,m) if op(A)=A and at least max(l,k) otherwise.</param>
		public void Bsrmm(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transB, int mb, int n, int kb, int nnzb,
									float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
									CudaDeviceVariable<int> bsrColIndA, int blockSize, CudaDeviceVariable<float> B, int ldb, float beta, CudaDeviceVariable<float> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseSbsrmm(_handle, dirA, transA, transB, mb, n, kb, nnzb, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer,
				bsrColIndA.DevicePointer, blockSize, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

		}

		/// <summary>
		/// This function performs one of the following matrix-matrix operations:
		/// C = alpha * op(A) * op(B) + beta * C
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrix op(B) and A.</param>
		/// <param name="kb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of non-zero blocks of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb + 1elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="B">array of dimensions (ldb, n) if op(B)=B and (ldb, k) otherwise.</param>
		/// <param name="ldb">leading dimension of B. If op(B)=B, it must be at least max(l,k) If op(B) != B, it must be at least
		/// max(1, n).</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max(l,m) if op(A)=A and at least max(l,k) otherwise.</param>
		public void Bsrmm(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transB, int mb, int n, int kb, int nnzb,
									double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
									CudaDeviceVariable<int> bsrColIndA, int blockSize, CudaDeviceVariable<double> B, int ldb, double beta, CudaDeviceVariable<double> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseDbsrmm(_handle, dirA, transA, transB, mb, n, kb, nnzb, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer,
				bsrColIndA.DevicePointer, blockSize, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

		}
		/// <summary>
		/// This function performs one of the following matrix-matrix operations:
		/// C = alpha * op(A) * op(B) + beta * C
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrix op(B) and A.</param>
		/// <param name="kb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of non-zero blocks of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb + 1elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="B">array of dimensions (ldb, n) if op(B)=B and (ldb, k) otherwise.</param>
		/// <param name="ldb">leading dimension of B. If op(B)=B, it must be at least max(l,k) If op(B) != B, it must be at least
		/// max(1, n).</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max(l,m) if op(A)=A and at least max(l,k) otherwise.</param>
		public void Bsrmm(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transB, int mb, int n, int kb, int nnzb,
									cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
									CudaDeviceVariable<int> bsrColIndA, int blockSize, CudaDeviceVariable<cuFloatComplex> B, int ldb, cuFloatComplex beta, CudaDeviceVariable<cuFloatComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseCbsrmm(_handle, dirA, transA, transB, mb, n, kb, nnzb, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer,
				bsrColIndA.DevicePointer, blockSize, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

		}
		/// <summary>
		/// This function performs one of the following matrix-matrix operations:
		/// C = alpha * op(A) * op(B) + beta * C
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrix op(B) and A.</param>
		/// <param name="kb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of non-zero blocks of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb + 1elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="B">array of dimensions (ldb, n) if op(B)=B and (ldb, k) otherwise.</param>
		/// <param name="ldb">leading dimension of B. If op(B)=B, it must be at least max(l,k) If op(B) != B, it must be at least
		/// max(1, n).</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max(l,m) if op(A)=A and at least max(l,k) otherwise.</param>
		public void Bsrmm(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transB, int mb, int n, int kb, int nnzb,
									cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
									CudaDeviceVariable<int> bsrColIndA, int blockSize, CudaDeviceVariable<cuDoubleComplex> B, int ldb, cuDoubleComplex beta, CudaDeviceVariable<cuDoubleComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseZbsrmm(_handle, dirA, transA, transB, mb, n, kb, nnzb, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer,
				bsrColIndA.DevicePointer, blockSize, B.DevicePointer, ldb, ref beta, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

		}



		/// <summary>
		/// This function performs the solve phase of the solution of a sparse triangular linear system:
		/// op(A) * op(Y) = alpha * op(X)
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(x) and op(Y).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of dense matrix Y and op(X).</param>
		/// <param name="nnzb">number of non-zero blocks of matrix A</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrVal">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="info">structure initialized using cusparseCreateBsrsm2Info().</param>
		/// <param name="X">right-hand-side array.</param>
		/// <param name="ldx">leading dimension of X. If op(X)=X, ldx&gt;=k; otherwise, ldx>=n.</param>
		/// <param name="Y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y. If op(A)=A, then ldc&gt;=m. If op(A)!=A, then ldx>=k.</param>
		/// <param name="policy">the supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Solve(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY, int mb, int n, int nnzb, float alpha,
											CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
											int blockSize, CudaSparseBsrsm2Info info, CudaDeviceVariable<float> X, int ldx, CudaDeviceVariable<float> Y, int ldy,
											cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseSbsrsm2_solve(_handle, dirA, transA, transXY, mb, n, nnzb, ref alpha, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer,
					bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, X.DevicePointer, ldx, Y.DevicePointer, ldy, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrsm2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the solve phase of the solution of a sparse triangular linear system:
		/// op(A) * op(Y) = alpha * op(X)
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(x) and op(Y).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of dense matrix Y and op(X).</param>
		/// <param name="nnzb">number of non-zero blocks of matrix A</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrVal">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="info">structure initialized using cusparseCreateBsrsm2Info().</param>
		/// <param name="X">right-hand-side array.</param>
		/// <param name="ldx">leading dimension of X. If op(X)=X, ldx&gt;=k; otherwise, ldx>=n.</param>
		/// <param name="Y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y. If op(A)=A, then ldc&gt;=m. If op(A)!=A, then ldx>=k.</param>
		/// <param name="policy">the supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Solve(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY, int mb, int n, int nnzb, double alpha,
											CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
											int blockSize, CudaSparseBsrsm2Info info, CudaDeviceVariable<double> X, int ldx, CudaDeviceVariable<double> Y, int ldy,
											cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseDbsrsm2_solve(_handle, dirA, transA, transXY, mb, n, nnzb, ref alpha, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer,
					bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, X.DevicePointer, ldx, Y.DevicePointer, ldy, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrsm2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the solve phase of the solution of a sparse triangular linear system:
		/// op(A) * op(Y) = alpha * op(X)
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(x) and op(Y).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of dense matrix Y and op(X).</param>
		/// <param name="nnzb">number of non-zero blocks of matrix A</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrVal">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="info">structure initialized using cusparseCreateBsrsm2Info().</param>
		/// <param name="X">right-hand-side array.</param>
		/// <param name="ldx">leading dimension of X. If op(X)=X, ldx&gt;=k; otherwise, ldx>=n.</param>
		/// <param name="Y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y. If op(A)=A, then ldc&gt;=m. If op(A)!=A, then ldx>=k.</param>
		/// <param name="policy">the supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Solve(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY, int mb, int n, int nnzb, cuFloatComplex alpha,
											CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
											int blockSize, CudaSparseBsrsm2Info info, CudaDeviceVariable<cuFloatComplex> X, int ldx, CudaDeviceVariable<cuFloatComplex> Y, int ldy,
											cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseCbsrsm2_solve(_handle, dirA, transA, transXY, mb, n, nnzb, ref alpha, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer,
					bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, X.DevicePointer, ldx, Y.DevicePointer, ldy, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrsm2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the solve phase of the solution of a sparse triangular linear system:
		/// op(A) * op(Y) = alpha * op(X)
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(x) and op(Y).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of dense matrix Y and op(X).</param>
		/// <param name="nnzb">number of non-zero blocks of matrix A</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrVal">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="info">structure initialized using cusparseCreateBsrsm2Info().</param>
		/// <param name="X">right-hand-side array.</param>
		/// <param name="ldx">leading dimension of X. If op(X)=X, ldx&gt;=k; otherwise, ldx>=n.</param>
		/// <param name="Y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y. If op(A)=A, then ldc&gt;=m. If op(A)!=A, then ldx>=k.</param>
		/// <param name="policy">the supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Solve(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY, int mb, int n, int nnzb, cuDoubleComplex alpha,
											CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
											int blockSize, CudaSparseBsrsm2Info info, CudaDeviceVariable<cuDoubleComplex> X, int ldx, CudaDeviceVariable<cuDoubleComplex> Y, int ldy,
											cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseZbsrsm2_solve(_handle, dirA, transA, transXY, mb, n, nnzb, ref alpha, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer,
					bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, X.DevicePointer, ldx, Y.DevicePointer, ldy, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrsm2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		#endregion

		#region ref device
		/// <summary>
		/// This function performs one of the following matrix-matrix operations:
		/// C = alpha * op(A) * op(B) + beta * C
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrix op(B) and A.</param>
		/// <param name="kb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of non-zero blocks of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb + 1elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="B">array of dimensions (ldb, n) if op(B)=B and (ldb, k) otherwise.</param>
		/// <param name="ldb">leading dimension of B. If op(B)=B, it must be at least max(l,k) If op(B) != B, it must be at least
		/// max(1, n).</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max(l,m) if op(A)=A and at least max(l,k) otherwise.</param>
		public void Bsrmm(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transB, int mb, int n, int kb, int nnzb,
									CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
									CudaDeviceVariable<int> bsrColIndA, int blockSize, CudaDeviceVariable<float> B, int ldb, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseSbsrmm(_handle, dirA, transA, transB, mb, n, kb, nnzb, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer,
				bsrColIndA.DevicePointer, blockSize, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

		}

		/// <summary>
		/// This function performs one of the following matrix-matrix operations:
		/// C = alpha * op(A) * op(B) + beta * C
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrix op(B) and A.</param>
		/// <param name="kb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of non-zero blocks of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb + 1elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="B">array of dimensions (ldb, n) if op(B)=B and (ldb, k) otherwise.</param>
		/// <param name="ldb">leading dimension of B. If op(B)=B, it must be at least max(l,k) If op(B) != B, it must be at least
		/// max(1, n).</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max(l,m) if op(A)=A and at least max(l,k) otherwise.</param>
		public void Bsrmm(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transB, int mb, int n, int kb, int nnzb,
									CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
									CudaDeviceVariable<int> bsrColIndA, int blockSize, CudaDeviceVariable<double> B, int ldb, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseDbsrmm(_handle, dirA, transA, transB, mb, n, kb, nnzb, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer,
				bsrColIndA.DevicePointer, blockSize, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

		}
		/// <summary>
		/// This function performs one of the following matrix-matrix operations:
		/// C = alpha * op(A) * op(B) + beta * C
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrix op(B) and A.</param>
		/// <param name="kb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of non-zero blocks of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb + 1elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="B">array of dimensions (ldb, n) if op(B)=B and (ldb, k) otherwise.</param>
		/// <param name="ldb">leading dimension of B. If op(B)=B, it must be at least max(l,k) If op(B) != B, it must be at least
		/// max(1, n).</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max(l,m) if op(A)=A and at least max(l,k) otherwise.</param>
		public void Bsrmm(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transB, int mb, int n, int kb, int nnzb,
									CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
									CudaDeviceVariable<int> bsrColIndA, int blockSize, CudaDeviceVariable<cuFloatComplex> B, int ldb, CudaDeviceVariable<cuFloatComplex> beta, CudaDeviceVariable<cuFloatComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseCbsrmm(_handle, dirA, transA, transB, mb, n, kb, nnzb, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer,
				bsrColIndA.DevicePointer, blockSize, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

		}
		/// <summary>
		/// This function performs one of the following matrix-matrix operations:
		/// C = alpha * op(A) * op(B) + beta * C
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="n">number of columns of dense matrix op(B) and A.</param>
		/// <param name="kb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of non-zero blocks of sparse matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb + 1elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="B">array of dimensions (ldb, n) if op(B)=B and (ldb, k) otherwise.</param>
		/// <param name="ldb">leading dimension of B. If op(B)=B, it must be at least max(l,k) If op(B) != B, it must be at least
		/// max(1, n).</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, C does not have to be a valid input.</param>
		/// <param name="C">array of dimensions (ldc, n).</param>
		/// <param name="ldc">leading dimension of C. It must be at least max(l,m) if op(A)=A and at least max(l,k) otherwise.</param>
		public void Bsrmm(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transB, int mb, int n, int kb, int nnzb,
									CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
									CudaDeviceVariable<int> bsrColIndA, int blockSize, CudaDeviceVariable<cuDoubleComplex> B, int ldb, CudaDeviceVariable<cuDoubleComplex> beta, CudaDeviceVariable<cuDoubleComplex> C, int ldc)
		{
			res = CudaSparseNativeMethods.cusparseZbsrmm(_handle, dirA, transA, transB, mb, n, kb, nnzb, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer,
				bsrColIndA.DevicePointer, blockSize, B.DevicePointer, ldb, beta.DevicePointer, C.DevicePointer, ldc);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrmm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);

		}



		/// <summary>
		/// This function performs the solve phase of the solution of a sparse triangular linear system:
		/// op(A) * op(Y) = alpha * op(X)
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(x) and op(Y).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of dense matrix Y and op(X).</param>
		/// <param name="nnzb">number of non-zero blocks of matrix A</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrVal">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="info">structure initialized using cusparseCreateBsrsm2Info().</param>
		/// <param name="X">right-hand-side array.</param>
		/// <param name="ldx">leading dimension of X. If op(X)=X, ldx&gt;=k; otherwise, ldx>=n.</param>
		/// <param name="Y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y. If op(A)=A, then ldc&gt;=m. If op(A)!=A, then ldx>=k.</param>
		/// <param name="policy">the supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Solve(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY, int mb, int n, int nnzb, CudaDeviceVariable<float> alpha,
											CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
											int blockSize, CudaSparseBsrsm2Info info, CudaDeviceVariable<float> X, int ldx, CudaDeviceVariable<float> Y, int ldy,
											cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseSbsrsm2_solve(_handle, dirA, transA, transXY, mb, n, nnzb, alpha.DevicePointer, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer,
					bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, X.DevicePointer, ldx, Y.DevicePointer, ldy, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrsm2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the solve phase of the solution of a sparse triangular linear system:
		/// op(A) * op(Y) = alpha * op(X)
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(x) and op(Y).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of dense matrix Y and op(X).</param>
		/// <param name="nnzb">number of non-zero blocks of matrix A</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrVal">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="info">structure initialized using cusparseCreateBsrsm2Info().</param>
		/// <param name="X">right-hand-side array.</param>
		/// <param name="ldx">leading dimension of X. If op(X)=X, ldx&gt;=k; otherwise, ldx>=n.</param>
		/// <param name="Y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y. If op(A)=A, then ldc&gt;=m. If op(A)!=A, then ldx>=k.</param>
		/// <param name="policy">the supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Solve(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY, int mb, int n, int nnzb, CudaDeviceVariable<double> alpha,
											CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
											int blockSize, CudaSparseBsrsm2Info info, CudaDeviceVariable<double> X, int ldx, CudaDeviceVariable<double> Y, int ldy,
											cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseDbsrsm2_solve(_handle, dirA, transA, transXY, mb, n, nnzb, alpha.DevicePointer, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer,
					bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, X.DevicePointer, ldx, Y.DevicePointer, ldy, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrsm2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the solve phase of the solution of a sparse triangular linear system:
		/// op(A) * op(Y) = alpha * op(X)
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(x) and op(Y).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of dense matrix Y and op(X).</param>
		/// <param name="nnzb">number of non-zero blocks of matrix A</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrVal">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="info">structure initialized using cusparseCreateBsrsm2Info().</param>
		/// <param name="X">right-hand-side array.</param>
		/// <param name="ldx">leading dimension of X. If op(X)=X, ldx&gt;=k; otherwise, ldx>=n.</param>
		/// <param name="Y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y. If op(A)=A, then ldc&gt;=m. If op(A)!=A, then ldx>=k.</param>
		/// <param name="policy">the supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Solve(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY, int mb, int n, int nnzb, CudaDeviceVariable<cuFloatComplex> alpha,
											CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
											int blockSize, CudaSparseBsrsm2Info info, CudaDeviceVariable<cuFloatComplex> X, int ldx, CudaDeviceVariable<cuFloatComplex> Y, int ldy,
											cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseCbsrsm2_solve(_handle, dirA, transA, transXY, mb, n, nnzb, alpha.DevicePointer, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer,
					bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, X.DevicePointer, ldx, Y.DevicePointer, ldy, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrsm2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the solve phase of the solution of a sparse triangular linear system:
		/// op(A) * op(Y) = alpha * op(X)
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transXY">the operation op(x) and op(Y).</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="n">number of columns of dense matrix Y and op(X).</param>
		/// <param name="nnzb">number of non-zero blocks of matrix A</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrVal">array of nnzb ( = bsrRowPtrA(mb) - 
		/// bsrRowPtrA(0) ) nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb + 1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb ( =bsrRowPtrA(mb) -
		/// bsrRowPtrA(0) ) column indices of the nonzero blocks of matrix A.</param>
		/// <param name="blockSize">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="info">structure initialized using cusparseCreateBsrsm2Info().</param>
		/// <param name="X">right-hand-side array.</param>
		/// <param name="ldx">leading dimension of X. If op(X)=X, ldx&gt;=k; otherwise, ldx>=n.</param>
		/// <param name="Y">solution array of dimensions (ldy, n).</param>
		/// <param name="ldy">leading dimension of Y. If op(A)=A, then ldc&gt;=m. If op(A)!=A, then ldx>=k.</param>
		/// <param name="policy">the supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by bsrsm2_bufferSizeExt().</param>
		public void Bsrsm2Solve(cusparseDirection dirA, cusparseOperation transA, cusparseOperation transXY, int mb, int n, int nnzb, CudaDeviceVariable<cuDoubleComplex> alpha,
											CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
											int blockSize, CudaSparseBsrsm2Info info, CudaDeviceVariable<cuDoubleComplex> X, int ldx, CudaDeviceVariable<cuDoubleComplex> Y, int ldy,
											cusparseSolvePolicy policy, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseZbsrsm2_solve(_handle, dirA, transA, transXY, mb, n, nnzb, alpha.DevicePointer, descrA.Descriptor, bsrVal.DevicePointer, bsrRowPtr.DevicePointer,
					bsrColInd.DevicePointer, blockSize, info.Bsrsm2Info, X.DevicePointer, ldx, Y.DevicePointer, ldy, policy, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrsm2_solve", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		#endregion















		/// <summary>
		/// This function computes the incomplete-LU factorization with 0 fill-in and no pivoting <para/>
		/// op(A) ≈ LU<para/>
		/// where A is m*n sparse matrix (that is defined in CSR storage format by the three arrays csrValM,
		/// csrRowPtrA and csrColIndA). <para/>
		/// Notice that the diagonal of lower triangular factor L is unitary and need not be stored.
		/// Therefore the input matrix is ovewritten with the resulting lower and upper triangular
		/// factor L and U, respectively.<para/>
		/// A call to this routine must be preceeded by a call to the csrsv_analysis routine.
		/// This function requires some extra storage. It is executed asynchronously with respect to
		/// the host and it may return control to the application on the host before the result is ready.
		/// </summary>
		/// <param name="trans">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="info">structure with information collected during the analysis phase (that
		/// should have been passed to the solve phase unchanged).</param>
		public void Csrilu0(cusparseOperation trans, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseScsrilu0(_handle, trans, m, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrilu0", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function computes the incomplete-LU factorization with 0 fill-in and no pivoting <para/>
		/// op(A) ≈ LU<para/>
		/// where A is m*n sparse matrix (that is defined in CSR storage format by the three arrays csrValM,
		/// csrRowPtrA and csrColIndA). <para/>
		/// Notice that the diagonal of lower triangular factor L is unitary and need not be stored.
		/// Therefore the input matrix is ovewritten with the resulting lower and upper triangular
		/// factor L and U, respectively.<para/>
		/// A call to this routine must be preceeded by a call to the csrsv_analysis routine.
		/// This function requires some extra storage. It is executed asynchronously with respect to
		/// the host and it may return control to the application on the host before the result is ready.
		/// </summary>
		/// <param name="trans">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="info">structure with information collected during the analysis phase (that
		/// should have been passed to the solve phase unchanged).</param>
		public void Csrilu0(cusparseOperation trans, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseDcsrilu0(_handle, trans, m, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrilu0", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function computes the incomplete-LU factorization with 0 fill-in and no pivoting <para/>
		/// op(A) ≈ LU<para/>
		/// where A is m*n sparse matrix (that is defined in CSR storage format by the three arrays csrValM,
		/// csrRowPtrA and csrColIndA). <para/>
		/// Notice that the diagonal of lower triangular factor L is unitary and need not be stored.
		/// Therefore the input matrix is ovewritten with the resulting lower and upper triangular
		/// factor L and U, respectively.<para/>
		/// A call to this routine must be preceeded by a call to the csrsv_analysis routine.
		/// This function requires some extra storage. It is executed asynchronously with respect to
		/// the host and it may return control to the application on the host before the result is ready.
		/// </summary>
		/// <param name="trans">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="info">structure with information collected during the analysis phase (that
		/// should have been passed to the solve phase unchanged).</param>
		public void Csrilu0(cusparseOperation trans, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseCcsrilu0(_handle, trans, m, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrilu0", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function computes the incomplete-LU factorization with 0 fill-in and no pivoting <para/>
		/// op(A) ≈ LU<para/>
		/// where A is m*n sparse matrix (that is defined in CSR storage format by the three arrays csrValM,
		/// csrRowPtrA and csrColIndA). <para/>
		/// Notice that the diagonal of lower triangular factor L is unitary and need not be stored.
		/// Therefore the input matrix is ovewritten with the resulting lower and upper triangular
		/// factor L and U, respectively.<para/>
		/// A call to this routine must be preceeded by a call to the csrsv_analysis routine.
		/// This function requires some extra storage. It is executed asynchronously with respect to
		/// the host and it may return control to the application on the host before the result is ready.
		/// </summary>
		/// <param name="trans">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="info">structure with information collected during the analysis phase (that
		/// should have been passed to the solve phase unchanged).</param>
		public void Csrilu0(cusparseOperation trans, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseZcsrilu0(_handle, trans, m, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrilu0", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// csrilu02(). To disable a boost value, the user can call csrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateCsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Csrilu02NumericBoost(csrilu02Info info, int enable_boost, CudaDeviceVariable<double> tol, CudaDeviceVariable<float> boost_val)
		{
			res = CudaSparseNativeMethods.cusparseScsrilu02_numericBoost(_handle, info, enable_boost, tol.DevicePointer, boost_val.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// csrilu02(). To disable a boost value, the user can call csrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateCsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Csrilu02NumericBoost(csrilu02Info info, int enable_boost, ref double tol, ref float boost_val)
		{
			res = CudaSparseNativeMethods.cusparseScsrilu02_numericBoost(_handle, info, enable_boost, ref tol, ref boost_val);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// csrilu02(). To disable a boost value, the user can call csrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateCsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Csrilu02NumericBoost(csrilu02Info info, int enable_boost, CudaDeviceVariable<double> tol, CudaDeviceVariable<double> boost_val)
		{
			res = CudaSparseNativeMethods.cusparseDcsrilu02_numericBoost(_handle, info, enable_boost, tol.DevicePointer, boost_val.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// csrilu02(). To disable a boost value, the user can call csrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateCsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Csrilu02NumericBoost(csrilu02Info info, int enable_boost, ref double tol, ref double boost_val)
		{
			res = CudaSparseNativeMethods.cusparseDcsrilu02_numericBoost(_handle, info, enable_boost, ref tol, ref boost_val);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// csrilu02(). To disable a boost value, the user can call csrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateCsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Csrilu02NumericBoost(csrilu02Info info, int enable_boost, CudaDeviceVariable<double> tol, CudaDeviceVariable<cuFloatComplex> boost_val)
		{
			res = CudaSparseNativeMethods.cusparseCcsrilu02_numericBoost(_handle, info, enable_boost, tol.DevicePointer, boost_val.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// csrilu02(). To disable a boost value, the user can call csrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateCsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Csrilu02NumericBoost(csrilu02Info info, int enable_boost, ref double tol, ref cuFloatComplex boost_val)
		{
			res = CudaSparseNativeMethods.cusparseCcsrilu02_numericBoost(_handle, info, enable_boost, ref tol, ref boost_val);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// csrilu02(). To disable a boost value, the user can call csrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateCsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Csrilu02NumericBoost(csrilu02Info info, int enable_boost, CudaDeviceVariable<double> tol, CudaDeviceVariable<cuDoubleComplex> boost_val)
		{
			res = CudaSparseNativeMethods.cusparseZcsrilu02_numericBoost(_handle, info, enable_boost, tol.DevicePointer, boost_val.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// csrilu02(). To disable a boost value, the user can call csrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateCsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Csrilu02NumericBoost(csrilu02Info info, int enable_boost, ref double tol, ref cuDoubleComplex boost_val)
		{
			res = CudaSparseNativeMethods.cusparseZcsrilu02_numericBoost(_handle, info, enable_boost, ref tol, ref boost_val);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		

		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXcsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called csrsv2_analysis() or csrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Csrilu02ZeroPivot(CudaSparseCsrilu02Info info, CudaDeviceVariable<int> position)
		{
			res = CudaSparseNativeMethods.cusparseXcsrilu02_zeroPivot(_handle, info.Csrilu02Info, position.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrilu02_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}
		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXcsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called csrsv2_analysis() or csrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Csrilu02ZeroPivot(CudaSparseCsrilu02Info info, ref int position)
		{
			res = CudaSparseNativeMethods.cusparseXcsrilu02_zeroPivot(_handle, info.Csrilu02Info, ref position);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrilu02_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}

		/// <summary>
		/// This function returns size of the buffer used in computing the incomplete-LU
		/// factorization with fill-in and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csrilu02BufferSize(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseScsrilu02_bufferSizeExt(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrilu02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of the buffer used in computing the incomplete-LU
		/// factorization with fill-in and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csrilu02BufferSize(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDcsrilu02_bufferSizeExt(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrilu02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of the buffer used in computing the incomplete-LU
		/// factorization with fill-in and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csrilu02BufferSize(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCcsrilu02_bufferSizeExt(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrilu02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of the buffer used in computing the incomplete-LU
		/// factorization with fill-in and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csrilu02BufferSize(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZcsrilu02_bufferSizeExt(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrilu02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		

		/// <summary>
		/// This function performs the analysis phase of the incomplete-LU factorization with fillin
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrilu02Analysis(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseScsrilu02_analysis(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrilu02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-LU factorization with fillin
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrilu02Analysis(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsrilu02_analysis(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrilu02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-LU factorization with fillin
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrilu02Analysis(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsrilu02_analysis(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrilu02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-LU factorization with fillin
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrilu02Analysis(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsrilu02_analysis(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrilu02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		

		/// <summary>
		/// This function performs the solve phase of the incomplete-LU factorization with fill-in
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrilu02(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info, 
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseScsrilu02(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrilu02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-LU factorization with fill-in
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrilu02(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info, 
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsrilu02(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrilu02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-LU factorization with fill-in
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrilu02(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info, 
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsrilu02(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrilu02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-LU factorization with fill-in
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csrilu02(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, 
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsrilu02Info info, 
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsrilu02(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrilu02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}










		
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// bsrilu02(). To disable a boost value, the user can call bsrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateBsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Bsrilu02NumericBoost(bsrilu02Info info, int enable_boost, CudaDeviceVariable<double> tol, CudaDeviceVariable<float> boost_val)
		{
			res = CudaSparseNativeMethods.cusparseSbsrilu02_numericBoost(_handle, info, enable_boost, tol.DevicePointer, boost_val.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// bsrilu02(). To disable a boost value, the user can call bsrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateBsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Bsrilu02NumericBoost(bsrilu02Info info, int enable_boost, ref double tol, ref float boost_val)
		{
			res = CudaSparseNativeMethods.cusparseSbsrilu02_numericBoost(_handle, info, enable_boost, ref tol, ref boost_val);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// bsrilu02(). To disable a boost value, the user can call bsrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateBsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Bsrilu02NumericBoost(bsrilu02Info info, int enable_boost, CudaDeviceVariable<double> tol, CudaDeviceVariable<double> boost_val)
		{
			res = CudaSparseNativeMethods.cusparseDbsrilu02_numericBoost(_handle, info, enable_boost, tol.DevicePointer, boost_val.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// bsrilu02(). To disable a boost value, the user can call bsrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateBsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Bsrilu02NumericBoost(bsrilu02Info info, int enable_boost, ref double tol, ref double boost_val)
		{
			res = CudaSparseNativeMethods.cusparseDbsrilu02_numericBoost(_handle, info, enable_boost, ref tol, ref boost_val);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// bsrilu02(). To disable a boost value, the user can call bsrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateBsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Bsrilu02NumericBoost(bsrilu02Info info, int enable_boost, CudaDeviceVariable<double> tol, CudaDeviceVariable<cuFloatComplex> boost_val)
		{
			res = CudaSparseNativeMethods.cusparseCbsrilu02_numericBoost(_handle, info, enable_boost, tol.DevicePointer, boost_val.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// bsrilu02(). To disable a boost value, the user can call bsrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateBsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Bsrilu02NumericBoost(bsrilu02Info info, int enable_boost, ref double tol, ref cuFloatComplex boost_val)
		{
			res = CudaSparseNativeMethods.cusparseCbsrilu02_numericBoost(_handle, info, enable_boost, ref tol, ref boost_val);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// bsrilu02(). To disable a boost value, the user can call bsrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateBsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Bsrilu02NumericBoost(bsrilu02Info info, int enable_boost, CudaDeviceVariable<double> tol, CudaDeviceVariable<cuDoubleComplex> boost_val)
		{
			res = CudaSparseNativeMethods.cusparseZbsrilu02_numericBoost(_handle, info, enable_boost, tol.DevicePointer, boost_val.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// The user can use a boost value to replace a numerical value in incomplete LU
		/// factorization. The tol is used to determine a numerical zero, and the boost_val is used
		/// to replace a numerical zero. The behavior is <para/>
		/// if tol >= fabs(A(j,j)), then A(j,j)=boost_val.<para/>
		/// To enable a boost value, the user has to set parameter enable_boost to 1 before calling
		/// bsrilu02(). To disable a boost value, the user can call bsrilu02_numericBoost()
		/// again with parameter enable_boost=0.<para/>
		/// If enable_boost=0, tol and boost_val are ignored.
		/// </summary>
		/// <param name="info">structure initialized using cusparseCreateBsrilu02Info().</param>
		/// <param name="enable_boost">disable boost by enable_boost=0; otherwise, boost is enabled.</param>
		/// <param name="tol">tolerance to determine a numerical zero.</param>
		/// <param name="boost_val">boost value to replace a numerical zero.</param>
		public void Bsrilu02NumericBoost(bsrilu02Info info, int enable_boost, ref double tol, ref cuDoubleComplex boost_val)
		{
			res = CudaSparseNativeMethods.cusparseZbsrilu02_numericBoost(_handle, info, enable_boost, ref tol, ref boost_val);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrilu02_numericBoost", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		

		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXbsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called bsrsv2_analysis() or bsrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Bsrilu02ZeroPivot(CudaSparseBsrilu02Info info, CudaDeviceVariable<int> position)
		{
			res = CudaSparseNativeMethods.cusparseXbsrilu02_zeroPivot(_handle, info.Bsrilu02Info, position.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXbsrilu02_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}
		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXbsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called bsrsv2_analysis() or bsrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Bsrilu02ZeroPivot(CudaSparseBsrilu02Info info, ref int position)
		{
			res = CudaSparseNativeMethods.cusparseXbsrilu02_zeroPivot(_handle, info.Bsrilu02Info, ref position);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXbsrilu02_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}

		/// <summary>
		/// This function returns size of the buffer used in computing the incomplete-LU
		/// factorization with fill-in and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public SizeT Bsrilu02BufferSize(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseSbsrilu02_bufferSizeExt(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrilu02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of the buffer used in computing the incomplete-LU
		/// factorization with fill-in and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public SizeT Bsrilu02BufferSize(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDbsrilu02_bufferSizeExt(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrilu02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of the buffer used in computing the incomplete-LU
		/// factorization with fill-in and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public SizeT Bsrilu02BufferSize(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCbsrilu02_bufferSizeExt(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrilu02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of the buffer used in computing the incomplete-LU
		/// factorization with fill-in and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public SizeT Bsrilu02BufferSize(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZbsrilu02_bufferSizeExt(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrilu02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		

		/// <summary>
		/// This function performs the analysis phase of the incomplete-LU factorization with 0 fillin
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsrilu02Analysis(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseSbsrilu02_analysis(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrilu02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-LU factorization with fillin
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsrilu02Analysis(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDbsrilu02_analysis(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrilu02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-LU factorization with fillin
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsrilu02Analysis(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCbsrilu02_analysis(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrilu02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-LU factorization with fillin
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsrilu02Analysis(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZbsrilu02_analysis(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrilu02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		

		/// <summary>
		/// This function performs the solve phase of the incomplete-LU factorization with fill-in
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA_ValM">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsrilu02(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA_ValM, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info, 
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseSbsrilu02(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA_ValM.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrilu02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-LU factorization with fill-in
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA_ValM">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsrilu02(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA_ValM, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info, 
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDbsrilu02(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA_ValM.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrilu02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-LU factorization with fill-in
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA_ValM">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsrilu02(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA_ValM, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info, 
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCbsrilu02(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA_ValM.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrilu02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-LU factorization with fill-in
		/// and no pivoting: A = LU
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA_ValM">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsrilu02(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA_ValM, CudaDeviceVariable<int> bsrRowPtrA, 
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsrilu02Info info, 
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZbsrilu02(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA_ValM.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsrilu02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrilu02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}






		/// <summary>
		/// This function computes the incomplete-Cholesky factorization with 0 fill-in and no pivoting <para/>
		/// op(A) ≈ R'R<para/>
		/// where A is m*n Hermitian/symmetric positive definite sparse matrix (that is defined in CSR storage format by the three arrays csrValM,
		/// csrRowPtrA and csrColIndA). <para/>
		/// Notice that only a lower or upper Hermitian/symmetric part of the matrix A is actually
		/// stored. It is overwritten by the lower or upper triangular factor R' or R, respectively.<para/>
		/// A call to this routine must be preceeded by a call to the csrsv_analysis routine.
		/// This function requires some extra storage. It is executed asynchronously with respect to
		/// the host and it may return control to the application on the host before the result is ready.
		/// </summary>
		/// <param name="trans">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="info">structure with information collected during the analysis phase (that
		/// should have been passed to the solve phase unchanged).</param>
		public void Csric0(cusparseOperation trans, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseScsric0(_handle, trans, m, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsric0", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function computes the incomplete-Cholesky factorization with 0 fill-in and no pivoting <para/>
		/// op(A) ≈ R'R<para/>
		/// where A is m*n Hermitian/symmetric positive definite sparse matrix (that is defined in CSR storage format by the three arrays csrValM,
		/// csrRowPtrA and csrColIndA). <para/>
		/// Notice that only a lower or upper Hermitian/symmetric part of the matrix A is actually
		/// stored. It is overwritten by the lower or upper triangular factor R' or R, respectively.<para/>
		/// A call to this routine must be preceeded by a call to the csrsv_analysis routine.
		/// This function requires some extra storage. It is executed asynchronously with respect to
		/// the host and it may return control to the application on the host before the result is ready.
		/// </summary>
		/// <param name="trans">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="info">structure with information collected during the analysis phase (that
		/// should have been passed to the solve phase unchanged).</param>
		public void Csric0(cusparseOperation trans, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseDcsric0(_handle, trans, m, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsric0", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function computes the incomplete-Cholesky factorization with 0 fill-in and no pivoting <para/>
		/// op(A) ≈ R'R<para/>
		/// where A is m*n Hermitian/symmetric positive definite sparse matrix (that is defined in CSR storage format by the three arrays csrValM,
		/// csrRowPtrA and csrColIndA). <para/>
		/// Notice that only a lower or upper Hermitian/symmetric part of the matrix A is actually
		/// stored. It is overwritten by the lower or upper triangular factor R' or R, respectively.<para/>
		/// A call to this routine must be preceeded by a call to the csrsv_analysis routine.
		/// This function requires some extra storage. It is executed asynchronously with respect to
		/// the host and it may return control to the application on the host before the result is ready.
		/// </summary>
		/// <param name="trans">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="info">structure with information collected during the analysis phase (that
		/// should have been passed to the solve phase unchanged).</param>
		public void Csric0(cusparseOperation trans, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseCcsric0(_handle, trans, m, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsric0", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function computes the incomplete-Cholesky factorization with 0 fill-in and no pivoting <para/>
		/// op(A) ≈ R'R<para/>
		/// where A is m*n Hermitian/symmetric positive definite sparse matrix (that is defined in CSR storage format by the three arrays csrValM,
		/// csrRowPtrA and csrColIndA). <para/>
		/// Notice that only a lower or upper Hermitian/symmetric part of the matrix A is actually
		/// stored. It is overwritten by the lower or upper triangular factor R' or R, respectively.<para/>
		/// A call to this routine must be preceeded by a call to the csrsv_analysis routine.
		/// This function requires some extra storage. It is executed asynchronously with respect to
		/// the host and it may return control to the application on the host before the result is ready.
		/// </summary>
		/// <param name="trans">the operation op(A).</param>
		/// <param name="m">number of rows and columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzz passed to CUSPARSE.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero
		/// elements of matrix A.</param>
		/// <param name="info">structure with information collected during the analysis phase (that
		/// should have been passed to the solve phase unchanged).</param>
		public void Csric0(cusparseOperation trans, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseSolveAnalysisInfo info)
		{
			res = CudaSparseNativeMethods.cusparseZcsric0(_handle, trans, m, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.SolveAnalysisInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsric0", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}









		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXcsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called csrsv2_analysis() or csrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Csric02ZeroPivot(CudaSparseCsric02Info info, CudaDeviceVariable<int> position)
		{
			res = CudaSparseNativeMethods.cusparseXcsric02_zeroPivot(_handle, info.Csric02Info, position.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsric02_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}
		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXcsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called csrsv2_analysis() or csrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Csric02ZeroPivot(CudaSparseCsric02Info info, ref int position)
		{
			res = CudaSparseNativeMethods.cusparseXcsric02_zeroPivot(_handle, info.Csric02Info, ref position);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsric02_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}

		/// <summary>
		/// This function returns size of buffer used in computing the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csric02BufferSize(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseScsric02_bufferSizeExt(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsric02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csric02BufferSize(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDcsric02_bufferSizeExt(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsric02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csric02BufferSize(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCcsric02_bufferSizeExt(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsric02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		public SizeT Csric02BufferSize(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZcsric02_bufferSizeExt(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsric02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}



		/// <summary>
		/// This function performs the analysis phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csric02Analysis(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseScsric02_analysis(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsric02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csric02Analysis(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsric02_analysis(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsric02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csric02Analysis(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsric02_analysis(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsric02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csric02Analysis(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsric02_analysis(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsric02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This function performs the solve phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csric02(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA,
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info,
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseScsric02(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsric02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csric02(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA,
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info,
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsric02(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsric02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csric02(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA,
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info,
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsric02(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsric02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="csrValA_ValM">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of csrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by csrsv2_bufferSizeExt().</param>
		public void Csric02(int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA_ValM, CudaDeviceVariable<int> csrRowPtrA,
			CudaDeviceVariable<int> csrColIndA, CudaSparseCsric02Info info,
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsric02(_handle, m, (int)csrColIndA.Size, descrA.Descriptor, csrValA_ValM.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, info.Csric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsric02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}













		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXbsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called bsrsv2_analysis() or bsrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Bsric02ZeroPivot(CudaSparseBsric02Info info, CudaDeviceVariable<int> position)
		{
			res = CudaSparseNativeMethods.cusparseXbsric02_zeroPivot(_handle, info.Bsric02Info, position.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXbsric02_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}
		/// <summary>
		/// If the returned error code is CUSPARSE_STATUS_ZERO_PIVOT, position=j means
		/// A(j,j) has either a structural zero or a numerical zero. Otherwise position=-1. <para/>
		/// The position can be 0-based or 1-based, the same as the matrix. <para/>
		/// Function cusparseXbsrsv2_zeroPivot() is a blocking call. It calls
		/// cudaDeviceSynchronize() to make sure all previous kernels are done. <para/>
		/// The position can be in the host memory or device memory. The user can set the proper
		/// mode with cusparseSetPointerMode().
		/// </summary>
		/// <param name="info">info contains structural zero or numerical zero if the user already called bsrsv2_analysis() or bsrsv2_solve().</param>
		/// <param name="position">if no structural or numerical zero, position is -1; otherwise, if A(j,j) is missing or U(j,j) is zero, position=j.</param>
		/// <returns>If true, position=j means A(j,j) has either a structural zero or a numerical zero; otherwise, position=-1.</returns>
		public bool Bsric02ZeroPivot(CudaSparseBsric02Info info, ref int position)
		{
			res = CudaSparseNativeMethods.cusparseXbsric02_zeroPivot(_handle, info.Bsric02Info, ref position);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXbsric02_zeroPivot", res));
			if (res == cusparseStatus.ZeroPivot) return true;
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return false;
		}

		/// <summary>
		/// This function returns size of buffer used in computing the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public SizeT Bsric02BufferSize(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseSbsric02_bufferSizeExt(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsric02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public SizeT Bsric02BufferSize(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDbsric02_bufferSizeExt(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsric02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public SizeT Bsric02BufferSize(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCbsric02_bufferSizeExt(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsric02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public SizeT Bsric02BufferSize(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZbsric02_bufferSizeExt(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsric02_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}



		/// <summary>
		/// This function performs the analysis phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsric02Analysis(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseSbsric02_analysis(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsric02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsric02Analysis(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDbsric02_analysis(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsric02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsric02Analysis(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCbsric02_analysis(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsric02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the analysis phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsric02Analysis(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info, cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZbsric02_analysis(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsric02_analysis", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This function performs the solve phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA_ValM">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsric02(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA_ValM, CudaDeviceVariable<int> bsrRowPtrA,
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info,
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseSbsric02(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA_ValM.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsric02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA_ValM">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsric02(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA_ValM, CudaDeviceVariable<int> bsrRowPtrA,
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info,
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDbsric02(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA_ValM.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsric02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA_ValM">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsric02(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA_ValM, CudaDeviceVariable<int> bsrRowPtrA,
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info,
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCbsric02(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA_ValM.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsric02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the solve phase of the incomplete-Cholesky
		/// factorization with fill-in and no pivoting: A = LL^H
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_TRIANGULAR and diagonal 
		/// types CUSPARSE_DIAG_TYPE_UNIT and CUSPARSE_DIAG_TYPE_NON_UNIT.</param>
		/// <param name="bsrValA_ValM">array of nnz (= bsrRowPtrA(m)-bsrRowPtrA(0)) non-zero elements of matrix A. <para/>Output: matrix containing the incomplete-LU lower and upper triangular factors.</param>
		/// <param name="bsrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnz (= bsrRowPtrA(m) - bsrRowPtrA(0)) column indices of the non-zero elements of matrix A.
		/// Length of bsrColIndA gives the number nzz passed to CUSPARSE. </param>
		/// <param name="info">record of internal states based on different algorithms.</param>
		/// <param name="policy">The supported policies are CUSPARSE_SOLVE_POLICY_NO_LEVEL and CUSPARSE_SOLVE_POLICY_USE_LEVEL.</param>
		/// <param name="buffer">buffer allocated by the user, the size is returned by bsrsv2_bufferSizeExt().</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		public void Bsric02(cusparseDirection dirA, int m, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA_ValM, CudaDeviceVariable<int> bsrRowPtrA,
			CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseBsric02Info info,
			cusparseSolvePolicy policy, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZbsric02(_handle, dirA, m, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA_ValM.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, info.Bsric02Info, policy, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsric02", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}






		/// <summary>
		/// Solution of tridiagonal linear system A * B = B, with multiple right-hand-sides. The coefficient matrix A is 
		/// composed of lower (dl), main (d) and upper (du) diagonals, and the right-hand-sides B are overwritten with the solution.
		/// </summary>
		/// <param name="m">the size of the linear system (must be >= 3).</param>
		/// <param name="n">number of right-hand-sides, columns of matrix B.</param>
		/// <param name="dl">dense array containing the lower diagonal of the tri-diagonal
		/// linear system. The first element of each lower diagonal must be zero.</param>
		/// <param name="d">dense array containing the main diagonal of the tri-diagonal linear system.</param>
		/// <param name="du">dense array containing the upper diagonal of the tri-diagonal linear system. The last element of each upper diagonal must be zero.</param>
		/// <param name="B">dense right-hand-side array of dimensions (ldb, m).</param>
		/// <param name="ldb">leading dimension of B (that is >= max(1;m)).</param>
		public void Gtsv(int m, int n, CudaDeviceVariable<float> dl, CudaDeviceVariable<float> d, CudaDeviceVariable<float> du, CudaDeviceVariable<float> B, int ldb)
		{
			res = CudaSparseNativeMethods.cusparseSgtsv(_handle, m, n, dl.DevicePointer, d.DevicePointer, du.DevicePointer, B.DevicePointer, ldb);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgtsv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of tridiagonal linear system A * B = B, with multiple right-hand-sides. The coefficient matrix A is 
		/// composed of lower (dl), main (d) and upper (du) diagonals, and the right-hand-sides B are overwritten with the solution.
		/// </summary>
		/// <param name="m">the size of the linear system (must be >= 3).</param>
		/// <param name="n">number of right-hand-sides, columns of matrix B.</param>
		/// <param name="dl">dense array containing the lower diagonal of the tri-diagonal
		/// linear system. The first element of each lower diagonal must be zero.</param>
		/// <param name="d">dense array containing the main diagonal of the tri-diagonal linear system.</param>
		/// <param name="du">dense array containing the upper diagonal of the tri-diagonal linear system. The last element of each upper diagonal must be zero.</param>
		/// <param name="B">dense right-hand-side array of dimensions (ldb, m).</param>
		/// <param name="ldb">leading dimension of B (that is >= max(1;m)).</param>
		public void Gtsv(int m, int n, CudaDeviceVariable<double> dl, CudaDeviceVariable<double> d, CudaDeviceVariable<double> du, CudaDeviceVariable<double> B, int ldb)
		{
			res = CudaSparseNativeMethods.cusparseDgtsv(_handle, m, n, dl.DevicePointer, d.DevicePointer, du.DevicePointer, B.DevicePointer, ldb);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgtsv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of tridiagonal linear system A * B = B, with multiple right-hand-sides. The coefficient matrix A is 
		/// composed of lower (dl), main (d) and upper (du) diagonals, and the right-hand-sides B are overwritten with the solution.
		/// </summary>
		/// <param name="m">the size of the linear system (must be >= 3).</param>
		/// <param name="n">number of right-hand-sides, columns of matrix B.</param>
		/// <param name="dl">dense array containing the lower diagonal of the tri-diagonal
		/// linear system. The first element of each lower diagonal must be zero.</param>
		/// <param name="d">dense array containing the main diagonal of the tri-diagonal linear system.</param>
		/// <param name="du">dense array containing the upper diagonal of the tri-diagonal linear system. The last element of each upper diagonal must be zero.</param>
		/// <param name="B">dense right-hand-side array of dimensions (ldb, m).</param>
		/// <param name="ldb">leading dimension of B (that is >= max(1;m)).</param>
		public void Gtsv(int m, int n, CudaDeviceVariable<cuFloatComplex> dl, CudaDeviceVariable<cuFloatComplex> d, CudaDeviceVariable<cuFloatComplex> du, CudaDeviceVariable<cuFloatComplex> B, int ldb)
		{
			res = CudaSparseNativeMethods.cusparseCgtsv(_handle, m, n, dl.DevicePointer, d.DevicePointer, du.DevicePointer, B.DevicePointer, ldb);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgtsv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of tridiagonal linear system A * B = B, with multiple right-hand-sides. The coefficient matrix A is 
		/// composed of lower (dl), main (d) and upper (du) diagonals, and the right-hand-sides B are overwritten with the solution.
		/// </summary>
		/// <param name="m">the size of the linear system (must be >= 3).</param>
		/// <param name="n">number of right-hand-sides, columns of matrix B.</param>
		/// <param name="dl">dense array containing the lower diagonal of the tri-diagonal
		/// linear system. The first element of each lower diagonal must be zero.</param>
		/// <param name="d">dense array containing the main diagonal of the tri-diagonal linear system.</param>
		/// <param name="du">dense array containing the upper diagonal of the tri-diagonal linear system. The last element of each upper diagonal must be zero.</param>
		/// <param name="B">dense right-hand-side array of dimensions (ldb, m).</param>
		/// <param name="ldb">leading dimension of B (that is >= max(1;m)).</param>
		public void Gtsv(int m, int n, CudaDeviceVariable<cuDoubleComplex> dl, CudaDeviceVariable<cuDoubleComplex> d, CudaDeviceVariable<cuDoubleComplex> du, CudaDeviceVariable<cuDoubleComplex> B, int ldb)
		{
			res = CudaSparseNativeMethods.cusparseZgtsv(_handle, m, n, dl.DevicePointer, d.DevicePointer, du.DevicePointer, B.DevicePointer, ldb);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgtsv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}





		/* Description: Solution of tridiagonal linear system A * B = B, 
	   with multiple right-hand-sides. The coefficient matrix A is 
	   composed of lower (dl), main (d) and upper (du) diagonals, and 
	   the right-hand-sides B are overwritten with the solution. 
	   These routines do not use pivoting, using a combination of PCR and CR algorithm */
		/// <summary/>
		public void Gtsv_nopivot(int m, int n, CudaDeviceVariable<float> dl, CudaDeviceVariable<float> d, CudaDeviceVariable<float> du, CudaDeviceVariable<float> B, int ldb)
		{
			res = CudaSparseNativeMethods.cusparseSgtsv_nopivot(_handle, m, n, dl.DevicePointer, d.DevicePointer, du.DevicePointer, B.DevicePointer, ldb);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgtsv_nopivot", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Gtsv_nopivot(int m, int n, CudaDeviceVariable<double> dl, CudaDeviceVariable<double> d, CudaDeviceVariable<double> du, CudaDeviceVariable<double> B, int ldb)
		{
			res = CudaSparseNativeMethods.cusparseDgtsv_nopivot(_handle, m, n, dl.DevicePointer, d.DevicePointer, du.DevicePointer, B.DevicePointer, ldb);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgtsv_nopivot", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Gtsv_nopivot(int m, int n, CudaDeviceVariable<cuFloatComplex> dl, CudaDeviceVariable<cuFloatComplex> d, CudaDeviceVariable<cuFloatComplex> du, CudaDeviceVariable<cuFloatComplex> B, int ldb)
		{
			res = CudaSparseNativeMethods.cusparseCgtsv_nopivot(_handle, m, n, dl.DevicePointer, d.DevicePointer, du.DevicePointer, B.DevicePointer, ldb);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgtsv_nopivot", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Gtsv_nopivot(int m, int n, CudaDeviceVariable<cuDoubleComplex> dl, CudaDeviceVariable<cuDoubleComplex> d, CudaDeviceVariable<cuDoubleComplex> du, CudaDeviceVariable<cuDoubleComplex> B, int ldb)
		{
			res = CudaSparseNativeMethods.cusparseZgtsv_nopivot(_handle, m, n, dl.DevicePointer, d.DevicePointer, du.DevicePointer, B.DevicePointer, ldb);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgtsv_nopivot", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}                               
                               






		/// <summary>
		/// Solution of a set of tridiagonal linear systems A * x = x, each with a single right-hand-side. The coefficient 
		/// matrices A are composed of lower (dl), main (d) and upper (du) diagonals and stored separated by a batchStride, while the 
		/// right-hand-sides x are also separated by a batchStride.
		/// </summary>
		/// <param name="m">the size of the linear system (must be >= 3).</param>
		/// <param name="dl">dense array containing the lower diagonal of the tri-diagonal 
		/// linear system. The lower diagonal dl(i) that corresponds to the ith linear system starts at location dl + batchStride * i in memory.
		/// Also, the first element of each lower diagonal must be zero.</param>
		/// <param name="d">dense array containing the main diagonal of the tri-diagonal linear system. The main diagonal d(i) that corresponds to the ith
		/// linear system starts at location d + batchStride * i in memory.</param>
		/// <param name="du">dense array containing the upper diagonal of the tri-diagonal linear system. The upper diagonal du(i) that corresponds to the ith
		/// linear system starts at location du + batchStride * i in memory. Also, the last element of each upper diagonal must be zero.</param>
		/// <param name="x">dense array that contains the right-hand-side of the tridiagonal linear system. The right-hand-side x(i) that corresponds 
		/// to the ith linear system starts at location x + batchStride * i in memory.</param>
		/// <param name="batchCount">Number of systems to solve.</param>
		/// <param name="batchStride">stride (number of elements) that separates the vectors of every system (must be at least m).</param>
		public void GtsvStridedBatch(int m, CudaDeviceVariable<float> dl, CudaDeviceVariable<float> d, CudaDeviceVariable<float> du, CudaDeviceVariable<float> x, int batchCount, int batchStride)
		{
			res = CudaSparseNativeMethods.cusparseSgtsvStridedBatch(_handle, m, dl.DevicePointer, d.DevicePointer, du.DevicePointer, x.DevicePointer, batchCount, batchStride);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgtsvStridedBatch", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of a set of tridiagonal linear systems A * x = x, each with a single right-hand-side. The coefficient 
		/// matrices A are composed of lower (dl), main (d) and upper (du) diagonals and stored separated by a batchStride, while the 
		/// right-hand-sides x are also separated by a batchStride.
		/// </summary>
		/// <param name="m">the size of the linear system (must be >= 3).</param>
		/// <param name="dl">dense array containing the lower diagonal of the tri-diagonal 
		/// linear system. The lower diagonal dl(i) that corresponds to the ith linear system starts at location dl + batchStride * i in memory.
		/// Also, the first element of each lower diagonal must be zero.</param>
		/// <param name="d">dense array containing the main diagonal of the tri-diagonal linear system. The main diagonal d(i) that corresponds to the ith
		/// linear system starts at location d + batchStride * i in memory.</param>
		/// <param name="du">dense array containing the upper diagonal of the tri-diagonal linear system. The upper diagonal du(i) that corresponds to the ith
		/// linear system starts at location du + batchStride * i in memory. Also, the last element of each upper diagonal must be zero.</param>
		/// <param name="x">dense array that contains the right-hand-side of the tridiagonal linear system. The right-hand-side x(i) that corresponds 
		/// to the ith linear system starts at location x + batchStride * i in memory.</param>
		/// <param name="batchCount">Number of systems to solve.</param>
		/// <param name="batchStride">stride (number of elements) that separates the vectors of every system (must be at least m).</param>
		public void GtsvStridedBatch(int m, CudaDeviceVariable<double> dl, CudaDeviceVariable<double> d, CudaDeviceVariable<double> du, CudaDeviceVariable<double> x, int batchCount, int batchStride)
		{
			res = CudaSparseNativeMethods.cusparseDgtsvStridedBatch(_handle, m, dl.DevicePointer, d.DevicePointer, du.DevicePointer, x.DevicePointer, batchCount, batchStride);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgtsvStridedBatch", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of a set of tridiagonal linear systems A * x = x, each with a single right-hand-side. The coefficient 
		/// matrices A are composed of lower (dl), main (d) and upper (du) diagonals and stored separated by a batchStride, while the 
		/// right-hand-sides x are also separated by a batchStride.
		/// </summary>
		/// <param name="m">the size of the linear system (must be >= 3).</param>
		/// <param name="dl">dense array containing the lower diagonal of the tri-diagonal 
		/// linear system. The lower diagonal dl(i) that corresponds to the ith linear system starts at location dl + batchStride * i in memory.
		/// Also, the first element of each lower diagonal must be zero.</param>
		/// <param name="d">dense array containing the main diagonal of the tri-diagonal linear system. The main diagonal d(i) that corresponds to the ith
		/// linear system starts at location d + batchStride * i in memory.</param>
		/// <param name="du">dense array containing the upper diagonal of the tri-diagonal linear system. The upper diagonal du(i) that corresponds to the ith
		/// linear system starts at location du + batchStride * i in memory. Also, the last element of each upper diagonal must be zero.</param>
		/// <param name="x">dense array that contains the right-hand-side of the tridiagonal linear system. The right-hand-side x(i) that corresponds 
		/// to the ith linear system starts at location x + batchStride * i in memory.</param>
		/// <param name="batchCount">Number of systems to solve.</param>
		/// <param name="batchStride">stride (number of elements) that separates the vectors of every system (must be at least m).</param>
		public void GtsvStridedBatch(int m, CudaDeviceVariable<cuFloatComplex> dl, CudaDeviceVariable<cuFloatComplex> d, CudaDeviceVariable<cuFloatComplex> du, CudaDeviceVariable<cuFloatComplex> x, int batchCount, int batchStride)
		{
			res = CudaSparseNativeMethods.cusparseCgtsvStridedBatch(_handle, m, dl.DevicePointer, d.DevicePointer, du.DevicePointer, x.DevicePointer, batchCount, batchStride);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgtsvStridedBatch", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// Solution of a set of tridiagonal linear systems A * x = x, each with a single right-hand-side. The coefficient 
		/// matrices A are composed of lower (dl), main (d) and upper (du) diagonals and stored separated by a batchStride, while the 
		/// right-hand-sides x are also separated by a batchStride.
		/// </summary>
		/// <param name="m">the size of the linear system (must be >= 3).</param>
		/// <param name="dl">dense array containing the lower diagonal of the tri-diagonal 
		/// linear system. The lower diagonal dl(i) that corresponds to the ith linear system starts at location dl + batchStride * i in memory.
		/// Also, the first element of each lower diagonal must be zero.</param>
		/// <param name="d">dense array containing the main diagonal of the tri-diagonal linear system. The main diagonal d(i) that corresponds to the ith
		/// linear system starts at location d + batchStride * i in memory.</param>
		/// <param name="du">dense array containing the upper diagonal of the tri-diagonal linear system. The upper diagonal du(i) that corresponds to the ith
		/// linear system starts at location du + batchStride * i in memory. Also, the last element of each upper diagonal must be zero.</param>
		/// <param name="x">dense array that contains the right-hand-side of the tridiagonal linear system. The right-hand-side x(i) that corresponds 
		/// to the ith linear system starts at location x + batchStride * i in memory.</param>
		/// <param name="batchCount">Number of systems to solve.</param>
		/// <param name="batchStride">stride (number of elements) that separates the vectors of every system (must be at least m).</param>
		public void GtsvStridedBatch(int m, CudaDeviceVariable<cuDoubleComplex> dl, CudaDeviceVariable<cuDoubleComplex> d, CudaDeviceVariable<cuDoubleComplex> du, CudaDeviceVariable<cuDoubleComplex> x, int batchCount, int batchStride)
		{
			res = CudaSparseNativeMethods.cusparseZgtsvStridedBatch(_handle, m, dl.DevicePointer, d.DevicePointer, du.DevicePointer, x.DevicePointer, batchCount, batchStride);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgtsvStridedBatch", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}




		/* --- Sparse Format Conversion --- */


		#region ref host
		/// <summary>
		/// This routine finds the total number of non-zero elements and the number of non-zero elements per row or column in the dense matrix A.
		/// </summary>
		/// <param name="dirA">direction that specifies whether to count non-zero elements by CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRowCol">Output: array of size m or n containing the number of non-zero elements per row or column, respectively.</param>
		/// <param name="nnzTotalDevHostPtr">Output: total number of non-zero elements in device or host memory.</param>
		public void Nnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<int> nnzPerRowCol, ref int nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseSnnz(_handle, dirA, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRowCol.DevicePointer, ref nnzTotalDevHostPtr);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSnnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine finds the total number of non-zero elements and the number of non-zero elements per row or column in the dense matrix A.
		/// </summary>
		/// <param name="dirA">direction that specifies whether to count non-zero elements by CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRowCol">Output: array of size m or n containing the number of non-zero elements per row or column, respectively.</param>
		/// <param name="nnzTotalDevHostPtr">Output: total number of non-zero elements in device or host memory.</param>
		public void Nnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<int> nnzPerRowCol, ref int nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseDnnz(_handle, dirA, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRowCol.DevicePointer, ref nnzTotalDevHostPtr);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDnnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine finds the total number of non-zero elements and the number of non-zero elements per row or column in the dense matrix A.
		/// </summary>
		/// <param name="dirA">direction that specifies whether to count non-zero elements by CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRowCol">Output: array of size m or n containing the number of non-zero elements per row or column, respectively.</param>
		/// <param name="nnzTotalDevHostPtr">Output: total number of non-zero elements in device or host memory.</param>
		public void Nnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> A, int lda, CudaDeviceVariable<int> nnzPerRowCol, ref int nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseCnnz(_handle, dirA, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRowCol.DevicePointer, ref nnzTotalDevHostPtr);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCnnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine finds the total number of non-zero elements and the number of non-zero elements per row or column in the dense matrix A.
		/// </summary>
		/// <param name="dirA">direction that specifies whether to count non-zero elements by CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRowCol">Output: array of size m or n containing the number of non-zero elements per row or column, respectively.</param>
		/// <param name="nnzTotalDevHostPtr">Output: total number of non-zero elements in device or host memory.</param>
		public void Nnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> A, int lda, CudaDeviceVariable<int> nnzPerRowCol, ref int nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseDnnz(_handle, dirA, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRowCol.DevicePointer, ref nnzTotalDevHostPtr);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDnnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		#endregion
		#region ref device
		/// <summary>
		/// This routine finds the total number of non-zero elements and the number of non-zero elements per row or column in the dense matrix A.
		/// </summary>
		/// <param name="dirA">direction that specifies whether to count non-zero elements by CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRowCol">Output: array of size m or n containing the number of non-zero elements per row or column, respectively.</param>
		/// <param name="nnzTotalDevHostPtr">Output: total number of non-zero elements in device or host memory.</param>
		public void Nnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<int> nnzPerRowCol, CudaDeviceVariable<int> nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseSnnz(_handle, dirA, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRowCol.DevicePointer, nnzTotalDevHostPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSnnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine finds the total number of non-zero elements and the number of non-zero elements per row or column in the dense matrix A.
		/// </summary>
		/// <param name="dirA">direction that specifies whether to count non-zero elements by CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRowCol">Output: array of size m or n containing the number of non-zero elements per row or column, respectively.</param>
		/// <param name="nnzTotalDevHostPtr">Output: total number of non-zero elements in device or host memory.</param>
		public void Nnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<int> nnzPerRowCol, CudaDeviceVariable<int> nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseDnnz(_handle, dirA, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRowCol.DevicePointer, nnzTotalDevHostPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDnnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine finds the total number of non-zero elements and the number of non-zero elements per row or column in the dense matrix A.
		/// </summary>
		/// <param name="dirA">direction that specifies whether to count non-zero elements by CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRowCol">Output: array of size m or n containing the number of non-zero elements per row or column, respectively.</param>
		/// <param name="nnzTotalDevHostPtr">Output: total number of non-zero elements in device or host memory.</param>
		public void Nnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> A, int lda, CudaDeviceVariable<int> nnzPerRowCol, CudaDeviceVariable<int> nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseCnnz(_handle, dirA, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRowCol.DevicePointer, nnzTotalDevHostPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCnnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine finds the total number of non-zero elements and the number of non-zero elements per row or column in the dense matrix A.
		/// </summary>
		/// <param name="dirA">direction that specifies whether to count non-zero elements by CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRowCol">Output: array of size m or n containing the number of non-zero elements per row or column, respectively.</param>
		/// <param name="nnzTotalDevHostPtr">Output: total number of non-zero elements in device or host memory.</param>
		public void Nnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> A, int lda, CudaDeviceVariable<int> nnzPerRowCol, CudaDeviceVariable<int> nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseZnnz(_handle, dirA, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRowCol.DevicePointer, nnzTotalDevHostPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZnnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		#endregion


		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in the CSR storage format, using the information computed by the nnz routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRow">array of size m containing the number of non-zero elements per row.</param>
		/// <param name="csrValA">Output: array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">Output: integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">Output: integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		public void Dense2csr(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<int> nnzPerRow, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			res = CudaSparseNativeMethods.cusparseSdense2csr(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRow.DevicePointer, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSdense2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in the CSR storage format, using the information computed by the nnz routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRow">array of size m containing the number of non-zero elements per row.</param>
		/// <param name="csrValA">Output: array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">Output: integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">Output: integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		public void Dense2csr(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<int> nnzPerRow, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			res = CudaSparseNativeMethods.cusparseDdense2csr(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRow.DevicePointer, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDdense2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in the CSR storage format, using the information computed by the nnz routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRow">array of size m containing the number of non-zero elements per row.</param>
		/// <param name="csrValA">Output: array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">Output: integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">Output: integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		public void Dense2csr(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> A, int lda, CudaDeviceVariable<int> nnzPerRow, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			res = CudaSparseNativeMethods.cusparseCdense2csr(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRow.DevicePointer, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCdense2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in the CSR storage format, using the information computed by the nnz routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRow">array of size m containing the number of non-zero elements per row.</param>
		/// <param name="csrValA">Output: array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">Output: integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">Output: integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		public void Dense2csr(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> A, int lda, CudaDeviceVariable<int> nnzPerRow, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			res = CudaSparseNativeMethods.cusparseZdense2csr(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRow.DevicePointer, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZdense2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This routine converts a sparse matrix in CSR storage format to a dense matrix.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="A">Output: array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">leading dimension of array matrix A.</param>
		public void Csr2dense(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<float> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseScsr2dense(_handle, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsr2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSR storage format to a dense matrix.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="A">Output: array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">leading dimension of array matrix A.</param>
		public void Csr2dense(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<double> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseDcsr2dense(_handle, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsr2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSR storage format to a dense matrix.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="A">Output: array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">leading dimension of array matrix A.</param>
		public void Csr2dense(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuFloatComplex> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseCcsr2dense(_handle, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsr2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSR storage format to a dense matrix.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="A">Output: array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">leading dimension of array matrix A.</param>
		public void Csr2dense(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuDoubleComplex> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseZcsr2dense(_handle, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsr2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in the CSC storage format, using the information computed by the nnz routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerCol">array of size n containing the number of non-zero elements per column.</param>
		/// <param name="cscValA">Output: array of nnz (= cscRowPtrA(m)-cscRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="cscRowIndA">Output: integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtrA">Output: integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		public void Dense2csc(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<int> nnzPerCol, CudaDeviceVariable<float> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA)
		{
			res = CudaSparseNativeMethods.cusparseSdense2csc(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerCol.DevicePointer, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSdense2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in the CSC storage format, using the information computed by the nnz routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerCol">array of size n containing the number of non-zero elements per column.</param>
		/// <param name="cscValA">Output: array of nnz (= cscRowPtrA(m)-cscRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="cscRowIndA">Output: integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtrA">Output: integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		public void Dense2csc(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<int> nnzPerCol, CudaDeviceVariable<double> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA)
		{
			res = CudaSparseNativeMethods.cusparseDdense2csc(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerCol.DevicePointer, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDdense2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in the CSC storage format, using the information computed by the nnz routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerCol">array of size n containing the number of non-zero elements per column.</param>
		/// <param name="cscValA">Output: array of nnz (= cscRowPtrA(m)-cscRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="cscRowIndA">Output: integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtrA">Output: integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		public void Dense2csc(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> A, int lda, CudaDeviceVariable<int> nnzPerCol, CudaDeviceVariable<cuFloatComplex> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA)
		{
			res = CudaSparseNativeMethods.cusparseCdense2csc(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerCol.DevicePointer, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCdense2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in the CSC storage format, using the information computed by the nnz routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerCol">array of size n containing the number of non-zero elements per column.</param>
		/// <param name="cscValA">Output: array of nnz (= cscRowPtrA(m)-cscRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="cscRowIndA">Output: integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtrA">Output: integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		public void Dense2csc(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> A, int lda, CudaDeviceVariable<int> nnzPerCol, CudaDeviceVariable<cuDoubleComplex> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA)
		{
			res = CudaSparseNativeMethods.cusparseZdense2csc(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerCol.DevicePointer, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZdense2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This routine converts a sparse matrix in CSC storage format to a dense matrix.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="cscValA">array of nnz (= cscRowPtrA(m)-cscRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="cscRowIndA">integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtrA">integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="A">Output: array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		public void Csc2dense(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA, CudaDeviceVariable<float> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseScsc2dense(_handle, m, n, descrA.Descriptor, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsc2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSC storage format to a dense matrix.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="cscValA">array of nnz (= cscRowPtrA(m)-cscRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="cscRowIndA">integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtrA">integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="A">Output: array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		public void Csc2dense(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA, CudaDeviceVariable<double> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseDcsc2dense(_handle, m, n, descrA.Descriptor, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsc2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSC storage format to a dense matrix.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="cscValA">array of nnz (= cscRowPtrA(m)-cscRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="cscRowIndA">integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtrA">integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="A">Output: array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		public void Csc2dense(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA, CudaDeviceVariable<cuFloatComplex> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseCcsc2dense(_handle, m, n, descrA.Descriptor, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsc2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSC storage format to a dense matrix.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="cscValA">array of nnz (= cscRowPtrA(m)-cscRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="cscRowIndA">integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtrA">integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="A">Output: array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		public void Csc2dense(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA, CudaDeviceVariable<cuDoubleComplex> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseZcsc2dense(_handle, m, n, descrA.Descriptor, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsc2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This routine compresses the indecis of rows or columns. It can be interpreted as a conversion from COO to CSR sparse storage format.
		/// </summary>
		/// <param name="cooRowInd">integer array of nnz uncompressed row indices. Length of cooRowInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="csrRowPtr">Output: integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="idxBase">Index base.</param>
		public void Xcoo2csr(CudaDeviceVariable<int> cooRowInd, int m, CudaDeviceVariable<int> csrRowPtr, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseXcoo2csr(_handle, cooRowInd.DevicePointer, (int)cooRowInd.Size, m, csrRowPtr.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcoo2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This routine uncompresses the indecis of rows or columns. It can be interpreted as a conversion from CSR to COO sparse storage format.
		/// </summary>
		/// <param name="csrRowPtr">Output: integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="cooRowInd">integer array of nnz uncompressed row indices. Length of cooRowInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="idxBase">Index base.</param>
		public void Xcsr2coo(CudaDeviceVariable<int> csrRowPtr, int m, CudaDeviceVariable<int> cooRowInd, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseXcoo2csr(_handle, csrRowPtr.DevicePointer, (int)cooRowInd.Size, m, cooRowInd.DevicePointer, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcoo2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This routine converts a matrix from CSR to CSC sparse storage format. The resulting matrix can be re-interpreted as a transpose of the original matrix in CSR storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="csrVal">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. Length of csrColInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="cscVal">Output: array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) nonzero elements of matrix A. It is only filled-in if copyValues is set
		/// to CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="cscRowInd">Output: integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtr">Output: integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="copyValues">CUSPARSE_ACTION_SYMBOLIC or CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="idxBase">Index base.</param>
		public void Csr2csc(int m, int n, CudaDeviceVariable<float> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaDeviceVariable<float> cscVal, CudaDeviceVariable<int> cscRowInd, CudaDeviceVariable<int> cscColPtr, cusparseAction copyValues, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseScsr2csc(_handle, m, n, (int)csrColInd.Size, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, cscVal.DevicePointer, cscRowInd.DevicePointer, cscColPtr.DevicePointer, copyValues, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsr2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a matrix from CSR to CSC sparse storage format. The resulting matrix can be re-interpreted as a transpose of the original matrix in CSR storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="csrVal">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. Length of csrColInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="cscVal">Output: array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) nonzero elements of matrix A. It is only filled-in if copyValues is set
		/// to CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="cscRowInd">Output: integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtr">Output: integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="copyValues">CUSPARSE_ACTION_SYMBOLIC or CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="idxBase">Index base.</param>
		public void Csr2csc(int m, int n, CudaDeviceVariable<double> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaDeviceVariable<double> cscVal, CudaDeviceVariable<int> cscRowInd, CudaDeviceVariable<int> cscColPtr, cusparseAction copyValues, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseDcsr2csc(_handle, m, n, (int)csrColInd.Size, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, cscVal.DevicePointer, cscRowInd.DevicePointer, cscColPtr.DevicePointer, copyValues, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsr2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a matrix from CSR to CSC sparse storage format. The resulting matrix can be re-interpreted as a transpose of the original matrix in CSR storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="csrVal">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. Length of csrColInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="cscVal">Output: array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) nonzero elements of matrix A. It is only filled-in if copyValues is set
		/// to CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="cscRowInd">Output: integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtr">Output: integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="copyValues">CUSPARSE_ACTION_SYMBOLIC or CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="idxBase">Index base.</param>
		public void Csr2csc(int m, int n, CudaDeviceVariable<cuFloatComplex> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaDeviceVariable<cuFloatComplex> cscVal, CudaDeviceVariable<int> cscRowInd, CudaDeviceVariable<int> cscColPtr, cusparseAction copyValues, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseCcsr2csc(_handle, m, n, (int)csrColInd.Size, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, cscVal.DevicePointer, cscRowInd.DevicePointer, cscColPtr.DevicePointer, copyValues, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsr2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a matrix from CSR to CSC sparse storage format. The resulting matrix can be re-interpreted as a transpose of the original matrix in CSR storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="csrVal">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A. Length of csrColInd gives the number nzz passed to CUSPARSE.</param>
		/// <param name="cscVal">Output: array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) nonzero elements of matrix A. It is only filled-in if copyValues is set
		/// to CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="cscRowInd">Output: integer array of nnz (= cscRowPtrA(m) - cscRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="cscColPtr">Output: integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="copyValues">CUSPARSE_ACTION_SYMBOLIC or CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="idxBase">Index base.</param>
		public void Csr2csc(int m, int n, CudaDeviceVariable<cuDoubleComplex> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaDeviceVariable<cuDoubleComplex> cscVal, CudaDeviceVariable<int> cscRowInd, CudaDeviceVariable<int> cscColPtr, cusparseAction copyValues, cusparseIndexBase idxBase)
		{
			res = CudaSparseNativeMethods.cusparseZcsr2csc(_handle, m, n, (int)csrColInd.Size, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, cscVal.DevicePointer, cscRowInd.DevicePointer, cscColPtr.DevicePointer, copyValues, idxBase);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsr2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in HYB storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of the dense matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRow">array of size m containing the number of non-zero elements per row.</param>
		/// <param name="hybA">Output: the matrix A in HYB storage format.</param>
		/// <param name="userEllWidth">width of the regular (ELL) part of the matrix in HYB format, which
		/// should be less than maximum number of non-zeros per row and is only required if partitionType == CUSPARSE_HYB_PARTITION_USER.</param>
		/// <param name="partitionType">partitioning method to be used in the conversion (please refer to cusparseHybPartition_t on page 15 for details).</param>
		public void Dense2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> A, int lda, CudaDeviceVariable<int> nnzPerRow, CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseSdense2hyb(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRow.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSdense2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in HYB storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of the dense matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRow">array of size m containing the number of non-zero elements per row.</param>
		/// <param name="hybA">Output: the matrix A in HYB storage format.</param>
		/// <param name="userEllWidth">width of the regular (ELL) part of the matrix in HYB format, which
		/// should be less than maximum number of non-zeros per row and is only required if partitionType == CUSPARSE_HYB_PARTITION_USER.</param>
		/// <param name="partitionType">partitioning method to be used in the conversion (please refer to cusparseHybPartition_t on page 15 for details).</param>
		public void Dense2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> A, int lda, CudaDeviceVariable<int> nnzPerRow, CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseDdense2hyb(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRow.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDdense2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in HYB storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of the dense matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRow">array of size m containing the number of non-zero elements per row.</param>
		/// <param name="hybA">Output: the matrix A in HYB storage format.</param>
		/// <param name="userEllWidth">width of the regular (ELL) part of the matrix in HYB format, which
		/// should be less than maximum number of non-zeros per row and is only required if partitionType == CUSPARSE_HYB_PARTITION_USER.</param>
		/// <param name="partitionType">partitioning method to be used in the conversion (please refer to cusparseHybPartition_t on page 15 for details).</param>
		public void Dense2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> A, int lda, CudaDeviceVariable<int> nnzPerRow, CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseCdense2hyb(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRow.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCdense2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a dense matrix to a sparse matrix in HYB storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of the dense matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="A">array of dimensions (lda, n).</param>
		/// <param name="lda">leading dimension of dense array A.</param>
		/// <param name="nnzPerRow">array of size m containing the number of non-zero elements per row.</param>
		/// <param name="hybA">Output: the matrix A in HYB storage format.</param>
		/// <param name="userEllWidth">width of the regular (ELL) part of the matrix in HYB format, which
		/// should be less than maximum number of non-zeros per row and is only required if partitionType == CUSPARSE_HYB_PARTITION_USER.</param>
		/// <param name="partitionType">partitioning method to be used in the conversion (please refer to cusparseHybPartition_t on page 15 for details).</param>
		public void Dense2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> A, int lda, CudaDeviceVariable<int> nnzPerRow, CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseZdense2hyb(_handle, m, n, descrA.Descriptor, A.DevicePointer, lda, nnzPerRow.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZdense2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This routine converts a sparse matrix in HYB storage format to a dense matrix.
		/// </summary>
		/// <param name="descrA">the descriptor of the matrix A in Hyb format. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="A">array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">the matrix A in HYB storage format.</param>
		public void Hyb2dense(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<float> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseShyb2dense(_handle, descrA.Descriptor, hybA.HybMat, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseShyb2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in HYB storage format to a dense matrix.
		/// </summary>
		/// <param name="descrA">the descriptor of the matrix A in Hyb format. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="A">array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">the matrix A in HYB storage format.</param>
		public void Hyb2dense(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<double> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseDhyb2dense(_handle, descrA.Descriptor, hybA.HybMat, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDhyb2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in HYB storage format to a dense matrix.
		/// </summary>
		/// <param name="descrA">the descriptor of the matrix A in Hyb format. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="A">array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">the matrix A in HYB storage format.</param>
		public void Hyb2dense(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuFloatComplex> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseChyb2dense(_handle, descrA.Descriptor, hybA.HybMat, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseChyb2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in HYB storage format to a dense matrix.
		/// </summary>
		/// <param name="descrA">the descriptor of the matrix A in Hyb format. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="A">array of dimensions (lda, n) that is filled in with the values of the sparse matrix.</param>
		/// <param name="lda">the matrix A in HYB storage format.</param>
		public void Hyb2dense(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuDoubleComplex> A, int lda)
		{
			res = CudaSparseNativeMethods.cusparseZhyb2dense(_handle, descrA.Descriptor, hybA.HybMat, A.DevicePointer, lda);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZhyb2dense", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This routine converts a sparse matrix in CSR storage format to a sparse matrix in HYB storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A in CSR format. The supported matrix type 
		/// is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="userEllWidth">width of the regular (ELL) part of the matrix in HYB format, which should be less than maximum number of non-zeros per row and is only
		/// required if partitionType == CUSPARSE_HYB_PARTITION_USER.</param>
		/// <param name="partitionType">partitioning method to be used in the conversion (please refer to cusparseHybPartition_t on page 15 for details).</param>
		public void Csr2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseScsr2hyb(_handle, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsr2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSR storage format to a sparse matrix in HYB storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A in CSR format. The supported matrix type 
		/// is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="userEllWidth">width of the regular (ELL) part of the matrix in HYB format, which should be less than maximum number of non-zeros per row and is only
		/// required if partitionType == CUSPARSE_HYB_PARTITION_USER.</param>
		/// <param name="partitionType">partitioning method to be used in the conversion (please refer to cusparseHybPartition_t on page 15 for details).</param>
		public void Csr2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseDcsr2hyb(_handle, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsr2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSR storage format to a sparse matrix in HYB storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A in CSR format. The supported matrix type 
		/// is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="userEllWidth">width of the regular (ELL) part of the matrix in HYB format, which should be less than maximum number of non-zeros per row and is only
		/// required if partitionType == CUSPARSE_HYB_PARTITION_USER.</param>
		/// <param name="partitionType">partitioning method to be used in the conversion (please refer to cusparseHybPartition_t on page 15 for details).</param>
		public void Csr2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseCcsr2hyb(_handle, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsr2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This routine converts a sparse matrix in CSR storage format to a sparse matrix in HYB storage format.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A in CSR format. The supported matrix type 
		/// is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m)-csrRowPtrA(0)) non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column indices of the non-zero elements of matrix A.</param>
		/// <param name="hybA">the matrix A in HYB storage format.</param>
		/// <param name="userEllWidth">width of the regular (ELL) part of the matrix in HYB format, which should be less than maximum number of non-zeros per row and is only
		/// required if partitionType == CUSPARSE_HYB_PARTITION_USER.</param>
		/// <param name="partitionType">partitioning method to be used in the conversion (please refer to cusparseHybPartition_t on page 15 for details).</param>
		public void Csr2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseZcsr2hyb(_handle, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsr2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		#endregion

		#region Sparse Level 4 routines
		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = op(A) * op(B) <para/>
		/// where op(A), op(B) and C are m x k, k x n, and m x n sparse matrices (defined in CSR
		/// storage format by the three arrays csrValA|csrValB|csrValC,
		/// csrRowPtrA|csrRowPtrB|csrRowPtrC, and csrColIndA|csrColIndB|csrcolIndC)
		/// respectively. <para/>
		/// Only support devices of compute capability 2.0 or above.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="m">number of rows of sparse matrix op(A) and C.</param>
		/// <param name="n">number of columns of sparse matrix op(B) and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix op(A) / op(B).</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrA">integer array of ~m + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~m = m if transA == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~m = k.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrB">integer array of ~k + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~k = k if transB == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~k = n.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="nnzTotalDevHostPtr"></param>
		public void CsrgemmNnz(cusparseOperation transA, cusparseOperation transB, int m, int n, int k, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseXcsrgemmNnz(_handle, transA, transB, m, n, k, descrA.Descriptor, (int)csrColIndA.Size, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrRowPtrC.DevicePointer, nnzTotalDevHostPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrgemmNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = op(A) * op(B) <para/>
		/// where op(A), op(B) and C are m x k, k x n, and m x n sparse matrices (defined in CSR
		/// storage format by the three arrays csrValA|csrValB|csrValC,
		/// csrRowPtrA|csrRowPtrB|csrRowPtrC, and csrColIndA|csrColIndB|csrcolIndC)
		/// respectively. <para/>
		/// Only support devices of compute capability 2.0 or above.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="m">number of rows of sparse matrix op(A) and C.</param>
		/// <param name="n">number of columns of sparse matrix op(B) and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix op(A) / op(B).</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrA">integer array of ~m + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~m = m if transA == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~m = k.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrB">integer array of ~k + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~k = k if transB == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~k = n.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="nnzTotalDevHostPtr"></param>
		public void CsrgemmNnz(cusparseOperation transA, cusparseOperation transB, int m, int n, int k, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> csrRowPtrC, ref int nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseXcsrgemmNnz(_handle, transA, transB, m, n, k, descrA.Descriptor, (int)csrColIndA.Size, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrRowPtrC.DevicePointer, ref nnzTotalDevHostPtr);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrgemmNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = op(A) * op(B) <para/>
		/// where op(A), op(B) and C are m x k, k x n, and m x n sparse matrices (defined in CSR
		/// storage format by the three arrays csrValA|csrValB|csrValC,
		/// csrRowPtrA|csrRowPtrB|csrRowPtrC, and csrColIndA|csrColIndB|csrcolIndC)
		/// respectively. <para/>
		/// Only support devices of compute capability 2.0 or above.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="m">number of rows of sparse matrix op(A) and C.</param>
		/// <param name="n">number of columns of sparse matrix op(B) and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix op(A) / op(B).</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of ~m + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~m = m if transA == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~m = k.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of ~k + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~k = k if transB == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~k = n.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgemm(cusparseOperation transA, cusparseOperation transB, int m, int n, int k, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<float> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseScsrgemm(_handle, transA, transB, m, n, k, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrgemm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = op(A) * op(B) <para/>
		/// where op(A), op(B) and C are m x k, k x n, and m x n sparse matrices (defined in CSR
		/// storage format by the three arrays csrValA|csrValB|csrValC,
		/// csrRowPtrA|csrRowPtrB|csrRowPtrC, and csrColIndA|csrColIndB|csrcolIndC)
		/// respectively. <para/>
		/// Only support devices of compute capability 2.0 or above.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="m">number of rows of sparse matrix op(A) and C.</param>
		/// <param name="n">number of columns of sparse matrix op(B) and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix op(A) / op(B).</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of ~m + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~m = m if transA == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~m = k.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of ~k + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~k = k if transB == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~k = n.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgemm(cusparseOperation transA, cusparseOperation transB, int m, int n, int k, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<double> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseDcsrgemm(_handle, transA, transB, m, n, k, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrgemm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = op(A) * op(B) <para/>
		/// where op(A), op(B) and C are m x k, k x n, and m x n sparse matrices (defined in CSR
		/// storage format by the three arrays csrValA|csrValB|csrValC,
		/// csrRowPtrA|csrRowPtrB|csrRowPtrC, and csrColIndA|csrColIndB|csrcolIndC)
		/// respectively. <para/>
		/// Only support devices of compute capability 2.0 or above.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="m">number of rows of sparse matrix op(A) and C.</param>
		/// <param name="n">number of columns of sparse matrix op(B) and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix op(A) / op(B).</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of ~m + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~m = m if transA == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~m = k.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of ~k + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~k = k if transB == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~k = n.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgemm(cusparseOperation transA, cusparseOperation transB, int m, int n, int k, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<cuFloatComplex> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseCcsrgemm(_handle, transA, transB, m, n, k, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrgemm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = op(A) * op(B) <para/>
		/// where op(A), op(B) and C are m x k, k x n, and m x n sparse matrices (defined in CSR
		/// storage format by the three arrays csrValA|csrValB|csrValC,
		/// csrRowPtrA|csrRowPtrB|csrRowPtrC, and csrColIndA|csrColIndB|csrcolIndC)
		/// respectively. <para/>
		/// Only support devices of compute capability 2.0 or above.
		/// </summary>
		/// <param name="transA">the operation op(A).</param>
		/// <param name="transB">the operation op(B).</param>
		/// <param name="m">number of rows of sparse matrix op(A) and C.</param>
		/// <param name="n">number of columns of sparse matrix op(B) and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix op(A) / op(B).</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of ~m + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~m = m if transA == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~m = k.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of ~k + 1 elements that contains the start of every row
		/// and the end of the last row plus one. ~k = k if transB == CUSPARSE_
		/// OPERATION_NON_TRANSPOSE, otherwise ~k = n.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgemm(cusparseOperation transA, cusparseOperation transB, int m, int n, int k, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<cuDoubleComplex> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseZcsrgemm(_handle, transA, transB, m, n, k, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrgemm", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <returns>number of bytes of the buffer used in csrgemm2Nnnz and csrgemm2.</returns>
		public SizeT Csrgemm2BufferSize(int m, int n, int k, float alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
										CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB, CudaDeviceVariable<int> csrSortedColIndB, float beta, CudaSparseMatrixDescriptor descrD,
										int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD, CudaDeviceVariable<int> csrSortedColIndD, CudaSparseCsrgemm2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseScsrgemm2_bufferSizeExt(_handle, m, n, k, ref alpha, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, ref beta, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer, csrSortedColIndD.DevicePointer, info.Csrgemm2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrgemm2_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <returns>number of bytes of the buffer used in csrgemm2Nnnz and csrgemm2.</returns>
		public SizeT Csrgemm2BufferSize(int m, int n, int k, double alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
										CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB, CudaDeviceVariable<int> csrSortedColIndB, double beta, CudaSparseMatrixDescriptor descrD,
										int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD, CudaDeviceVariable<int> csrSortedColIndD, CudaSparseCsrgemm2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDcsrgemm2_bufferSizeExt(_handle, m, n, k, ref alpha, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, ref beta, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer, csrSortedColIndD.DevicePointer, info.Csrgemm2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrgemm2_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <returns>number of bytes of the buffer used in csrgemm2Nnnz and csrgemm2.</returns>
		public SizeT Csrgemm2BufferSize(int m, int n, int k, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
										CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB, CudaDeviceVariable<int> csrSortedColIndB, cuFloatComplex beta, CudaSparseMatrixDescriptor descrD,
										int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD, CudaDeviceVariable<int> csrSortedColIndD, CudaSparseCsrgemm2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCcsrgemm2_bufferSizeExt(_handle, m, n, k, ref alpha, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, ref beta, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer, csrSortedColIndD.DevicePointer, info.Csrgemm2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrgemm2_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <returns>number of bytes of the buffer used in csrgemm2Nnnz and csrgemm2.</returns>
		public SizeT Csrgemm2BufferSize(int m, int n, int k, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
										CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB, CudaDeviceVariable<int> csrSortedColIndB, cuDoubleComplex beta, CudaSparseMatrixDescriptor descrD,
										int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD, CudaDeviceVariable<int> csrSortedColIndD, CudaSparseCsrgemm2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZcsrgemm2_bufferSizeExt(_handle, m, n, k, ref alpha, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, ref beta, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer, csrSortedColIndD.DevicePointer, info.Csrgemm2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrgemm2_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}


		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <returns>number of bytes of the buffer used in csrgemm2Nnnz and csrgemm2.</returns>
		public SizeT Csrgemm2BufferSize(int m, int n, int k, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
										CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB, CudaDeviceVariable<int> csrSortedColIndB, CudaDeviceVariable<float> beta, CudaSparseMatrixDescriptor descrD,
										int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD, CudaDeviceVariable<int> csrSortedColIndD, CudaSparseCsrgemm2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseScsrgemm2_bufferSizeExt(_handle, m, n, k, alpha.DevicePointer, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, beta.DevicePointer, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer, csrSortedColIndD.DevicePointer, info.Csrgemm2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrgemm2_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <returns>number of bytes of the buffer used in csrgemm2Nnnz and csrgemm2.</returns>
		public SizeT Csrgemm2BufferSize(int m, int n, int k, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
										CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB, CudaDeviceVariable<int> csrSortedColIndB, CudaDeviceVariable<double> beta, CudaSparseMatrixDescriptor descrD,
										int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD, CudaDeviceVariable<int> csrSortedColIndD, CudaSparseCsrgemm2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDcsrgemm2_bufferSizeExt(_handle, m, n, k, alpha.DevicePointer, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, beta.DevicePointer, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer, csrSortedColIndD.DevicePointer, info.Csrgemm2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrgemm2_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <returns>number of bytes of the buffer used in csrgemm2Nnnz and csrgemm2.</returns>
		public SizeT Csrgemm2BufferSize(int m, int n, int k, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
										CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB, CudaDeviceVariable<int> csrSortedColIndB, CudaDeviceVariable<cuFloatComplex> beta, CudaSparseMatrixDescriptor descrD,
										int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD, CudaDeviceVariable<int> csrSortedColIndD, CudaSparseCsrgemm2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCcsrgemm2_bufferSizeExt(_handle, m, n, k, alpha.DevicePointer, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, beta.DevicePointer, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer, csrSortedColIndD.DevicePointer, info.Csrgemm2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrgemm2_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <returns>number of bytes of the buffer used in csrgemm2Nnnz and csrgemm2.</returns>
		public SizeT Csrgemm2BufferSize(int m, int n, int k, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
										CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB, CudaDeviceVariable<int> csrSortedColIndB, CudaDeviceVariable<cuDoubleComplex> beta, CudaSparseMatrixDescriptor descrD,
										int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD, CudaDeviceVariable<int> csrSortedColIndD, CudaSparseCsrgemm2Info info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZcsrgemm2_bufferSizeExt(_handle, m, n, k, alpha.DevicePointer, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, beta.DevicePointer, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer, csrSortedColIndD.DevicePointer, info.Csrgemm2Info, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrgemm2_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedValA">array of nnzA nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedValB">array of nnzB nonzero elements of matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedValD">array of nnzD nonzero elements of matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedValC">array of nnzC nonzero elements of matrix C.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndC">integer array of nnzC column indices of the nonzero elements of matrix C.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2(int m, int n, int k, float alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<float> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<float> csrSortedValB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, float beta, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<float> csrSortedValD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> csrSortedValC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> csrSortedColIndC,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseScsrgemm2(_handle, m, n, k, ref alpha, descrA.Descriptor, nnzA, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedValB.DevicePointer, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, ref beta, descrD.Descriptor, nnzD, csrSortedValD.DevicePointer, csrSortedRowPtrD.DevicePointer, 
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedValC.DevicePointer, csrSortedRowPtrC.DevicePointer, csrSortedColIndC.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrgemm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedValA">array of nnzA nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedValB">array of nnzB nonzero elements of matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedValD">array of nnzD nonzero elements of matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedValC">array of nnzC nonzero elements of matrix C.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndC">integer array of nnzC column indices of the nonzero elements of matrix C.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2(int m, int n, int k, double alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<double> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<double> csrSortedValB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, double beta, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<double> csrSortedValD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> csrSortedValC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> csrSortedColIndC,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsrgemm2(_handle, m, n, k, ref alpha, descrA.Descriptor, nnzA, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedValB.DevicePointer, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, ref beta, descrD.Descriptor, nnzD, csrSortedValD.DevicePointer, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedValC.DevicePointer, csrSortedRowPtrC.DevicePointer, csrSortedColIndC.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrgemm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedValA">array of nnzA nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedValB">array of nnzB nonzero elements of matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedValD">array of nnzD nonzero elements of matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedValC">array of nnzC nonzero elements of matrix C.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndC">integer array of nnzC column indices of the nonzero elements of matrix C.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2(int m, int n, int k, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<cuFloatComplex> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<cuFloatComplex> csrSortedValB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, cuFloatComplex beta, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<cuFloatComplex> csrSortedValD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> csrSortedValC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> csrSortedColIndC,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsrgemm2(_handle, m, n, k, ref alpha, descrA.Descriptor, nnzA, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedValB.DevicePointer, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, ref beta, descrD.Descriptor, nnzD, csrSortedValD.DevicePointer, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedValC.DevicePointer, csrSortedRowPtrC.DevicePointer, csrSortedColIndC.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrgemm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedValA">array of nnzA nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedValB">array of nnzB nonzero elements of matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedValD">array of nnzD nonzero elements of matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedValC">array of nnzC nonzero elements of matrix C.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndC">integer array of nnzC column indices of the nonzero elements of matrix C.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2(int m, int n, int k, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<cuDoubleComplex> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<cuDoubleComplex> csrSortedValB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, cuDoubleComplex beta, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<cuDoubleComplex> csrSortedValD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> csrSortedValC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> csrSortedColIndC,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsrgemm2(_handle, m, n, k, ref alpha, descrA.Descriptor, nnzA, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedValB.DevicePointer, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, ref beta, descrD.Descriptor, nnzD, csrSortedValD.DevicePointer, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedValC.DevicePointer, csrSortedRowPtrC.DevicePointer, csrSortedColIndC.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrgemm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedValA">array of nnzA nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedValB">array of nnzB nonzero elements of matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedValD">array of nnzD nonzero elements of matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedValC">array of nnzC nonzero elements of matrix C.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndC">integer array of nnzC column indices of the nonzero elements of matrix C.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2(int m, int n, int k, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<float> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<float> csrSortedValB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, CudaDeviceVariable<float> beta, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<float> csrSortedValD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> csrSortedValC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> csrSortedColIndC,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseScsrgemm2(_handle, m, n, k, alpha.DevicePointer, descrA.Descriptor, nnzA, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedValB.DevicePointer, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, beta.DevicePointer, descrD.Descriptor, nnzD, csrSortedValD.DevicePointer, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedValC.DevicePointer, csrSortedRowPtrC.DevicePointer, csrSortedColIndC.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrgemm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedValA">array of nnzA nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedValB">array of nnzB nonzero elements of matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedValD">array of nnzD nonzero elements of matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedValC">array of nnzC nonzero elements of matrix C.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndC">integer array of nnzC column indices of the nonzero elements of matrix C.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2(int m, int n, int k, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<double> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<double> csrSortedValB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, CudaDeviceVariable<double> beta, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<double> csrSortedValD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> csrSortedValC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> csrSortedColIndC,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsrgemm2(_handle, m, n, k, alpha.DevicePointer, descrA.Descriptor, nnzA, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedValB.DevicePointer, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, beta.DevicePointer, descrD.Descriptor, nnzD, csrSortedValD.DevicePointer, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedValC.DevicePointer, csrSortedRowPtrC.DevicePointer, csrSortedColIndC.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrgemm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedValA">array of nnzA nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedValB">array of nnzB nonzero elements of matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedValD">array of nnzD nonzero elements of matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedValC">array of nnzC nonzero elements of matrix C.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndC">integer array of nnzC column indices of the nonzero elements of matrix C.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2(int m, int n, int k, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<cuFloatComplex> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<cuFloatComplex> csrSortedValB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, CudaDeviceVariable<cuFloatComplex> beta, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<cuFloatComplex> csrSortedValD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> csrSortedValC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> csrSortedColIndC,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsrgemm2(_handle, m, n, k, alpha.DevicePointer, descrA.Descriptor, nnzA, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedValB.DevicePointer, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, beta.DevicePointer, descrD.Descriptor, nnzD, csrSortedValD.DevicePointer, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedValC.DevicePointer, csrSortedRowPtrC.DevicePointer, csrSortedColIndC.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrgemm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedValA">array of nnzA nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedValB">array of nnzB nonzero elements of matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedValD">array of nnzD nonzero elements of matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedValC">array of nnzC nonzero elements of matrix C.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndC">integer array of nnzC column indices of the nonzero elements of matrix C.</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2(int m, int n, int k, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<cuDoubleComplex> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<cuDoubleComplex> csrSortedValB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, CudaDeviceVariable<cuDoubleComplex> beta, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<cuDoubleComplex> csrSortedValD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> csrSortedValC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> csrSortedColIndC,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsrgemm2(_handle, m, n, k, alpha.DevicePointer, descrA.Descriptor, nnzA, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedValB.DevicePointer, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, beta.DevicePointer, descrD.Descriptor, nnzD, csrSortedValD.DevicePointer, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedValC.DevicePointer, csrSortedRowPtrC.DevicePointer, csrSortedColIndC.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrgemm2", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="nnzTotalDevHostPtr">total number of nonzero elements in device or host memory. It is equal to (csrRowPtrC(m)-csrRowPtrC(0)).</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2Nnz(int m, int n, int k, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> csrSortedRowPtrC, CudaDeviceVariable<int> nnzTotalDevHostPtr,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseXcsrgemm2Nnz(_handle, m, n, k, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedRowPtrC.DevicePointer, nnzTotalDevHostPtr.DevicePointer, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrgemm2Nnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation:<para/>
		/// C = alpha * A *A B + beta * D<para/>
		/// where A, B, D and C are m×k, k×n, m×n and m×n sparse matrices (defined in CSR storage
		/// format by the three arrays csrValA|csrValB|csrValD|csrValC, csrRowPtrA|
		/// csrRowPtrB|csrRowPtrD|csrRowPtrC, and csrColIndA|csrColIndB|csrColIndD|csrcolIndC respectively.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A, D and C.</param>
		/// <param name="n">number of columns of sparse matrix B, D and C.</param>
		/// <param name="k">number of columns/rows of sparse matrix A / B.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzA">number of nonzero elements of sparse matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnzA column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only</param>
		/// <param name="nnzB">number of nonzero elements of sparse matrix B.</param>
		/// <param name="csrSortedRowPtrB">integer array of k+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndB">integer array of nnzB column indices of the nonzero elements of matrix B.</param>
		/// <param name="descrD">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="nnzD">number of nonzero elements of sparse matrix D.</param>
		/// <param name="csrSortedRowPtrD">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndD">integer array of nnzD column indices of the nonzero elements of matrix D.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrSortedRowPtrC">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="nnzTotalDevHostPtr">total number of nonzero elements in device or host memory. It is equal to (csrRowPtrC(m)-csrRowPtrC(0)).</param>
		/// <param name="info">structure with information used in csrgemm2Nnz and csrgemm2.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by csrgemm2BufferSize</param>
		public void Csrgemm2Nnz(int m, int n, int k, CudaSparseMatrixDescriptor descrA, int nnzA, CudaDeviceVariable<int> csrSortedRowPtrA,
							CudaDeviceVariable<int> csrSortedColIndA, CudaSparseMatrixDescriptor descrB, int nnzB, CudaDeviceVariable<int> csrSortedRowPtrB,
							CudaDeviceVariable<int> csrSortedColIndB, CudaSparseMatrixDescriptor descrD, int nnzD, CudaDeviceVariable<int> csrSortedRowPtrD,
							CudaDeviceVariable<int> csrSortedColIndD, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> csrSortedRowPtrC, ref int nnzTotalDevHostPtr,
							CudaSparseCsrgemm2Info info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseXcsrgemm2Nnz(_handle, m, n, k, descrA.Descriptor, nnzA, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				descrB.Descriptor, nnzB, csrSortedRowPtrB.DevicePointer, csrSortedColIndB.DevicePointer, descrD.Descriptor, nnzD, csrSortedRowPtrD.DevicePointer,
				csrSortedColIndD.DevicePointer, descrC.Descriptor, csrSortedRowPtrC.DevicePointer, ref nnzTotalDevHostPtr, info.Csrgemm2Info, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrgemm2Nnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}






		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="nnzTotalDevHostPtr"></param>
		public void CsrgeamNnz(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseXcsrgeamNnz(_handle, m, n, descrA.Descriptor, (int)csrColIndA.Size, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrRowPtrC.DevicePointer, nnzTotalDevHostPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrgeamNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="nnzTotalDevHostPtr"></param>
		public void CsrgeamNnz(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> csrRowPtrC, ref int nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseXcsrgeamNnz(_handle, m, n, descrA.Descriptor, (int)csrColIndA.Size, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrRowPtrC.DevicePointer, ref nnzTotalDevHostPtr);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrgeamNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="alpha">scalar used for multiplication.</param> 
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgeam(int m, int n, float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, float beta, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<float> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseScsrgeam(_handle, m, n, ref alpha, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, ref beta, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrgeam", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="alpha">scalar used for multiplication.</param> 
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgeam(int m, int n, double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, double beta, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<double> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseDcsrgeam(_handle, m, n, ref alpha, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, ref beta, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrgeam", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="alpha">scalar used for multiplication.</param> 
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgeam(int m, int n, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, cuFloatComplex beta, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<cuFloatComplex> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseCcsrgeam(_handle, m, n, ref alpha, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, ref beta, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrgeam", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="alpha">scalar used for multiplication.</param> 
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgeam(int m, int n, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, cuDoubleComplex beta, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<cuDoubleComplex> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseZcsrgeam(_handle, m, n, ref alpha, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, ref beta, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrgeam", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="alpha">scalar used for multiplication.</param> 
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgeam(int m, int n, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<float> beta, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<float> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseScsrgeam(_handle, m, n, alpha.DevicePointer, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, beta.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrgeam", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="alpha">scalar used for multiplication.</param> 
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgeam(int m, int n, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<double> beta, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<double> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseDcsrgeam(_handle, m, n, alpha.DevicePointer, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, beta.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrgeam", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="alpha">scalar used for multiplication.</param> 
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgeam(int m, int n, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuFloatComplex> beta, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<cuFloatComplex> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseCcsrgeam(_handle, m, n, alpha.DevicePointer, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, beta.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrgeam", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs following matrix-matrix operation<para/>
		/// C = alpha * A + beta * B <para/>
		/// where A, B and C are m x n sparse matrices (defined in CSR storage format by the three
		/// arrays csrValA|csrValB|csrValC, csrRowPtrA|csrRowPtrB|csrRowPtrC, and
		/// csrColIndA|csrColIndB|csrcolIndC respectively), and alpha and beta are scalars. Since A and
		/// B have different sparsity patterns, CUSPARSE adopts two-step approach to complete
		/// sparse matrix C.
		/// </summary>
		/// <param name="m">number of rows of sparse matrix A,B,C.</param>
		/// <param name="n">number of columns of sparse matrix A,B,C.</param>
		/// <param name="alpha">scalar used for multiplication.</param> 
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_
		/// MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValA">array of nnzA non-zero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnzA column indices of the non-zero elements of matrix A. Length of csrColIndA gives the number nzzA passed to CUSPARSE.</param>
		/// <param name="beta">scalar used for multiplication.</param>
		/// <param name="descrB">the descriptor of matrix B. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValB">array of nnzB non-zero elements of matrix B.</param>
		/// <param name="csrRowPtrB">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndB">integer array of nnzB column indices of the non-zero elements of matrix B. Length of csrColIndB gives the number nzzB passed to CUSPARSE.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL only.</param>
		/// <param name="csrValC">array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnzC (= csrRowPtrC(m) - csrRowPtrC(0)) column indices of the non-zero elements of matrix C.</param>
		public void Csrgeam(int m, int n, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<cuDoubleComplex> beta, CudaSparseMatrixDescriptor descrB, CudaDeviceVariable<cuDoubleComplex> csrValB, CudaDeviceVariable<int> csrRowPtrB, CudaDeviceVariable<int> csrColIndB, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseZcsrgeam(_handle, m, n, alpha.DevicePointer, descrA.Descriptor, (int)csrColIndA.Size, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, beta.DevicePointer, descrB.Descriptor, (int)csrColIndB.Size, csrValB.DevicePointer, csrRowPtrB.DevicePointer, csrColIndB.DevicePointer, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrgeam", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This function performs the coloring of the adjacency graph associated with the matrix
		/// A stored in CSR format. The coloring is an assignment of colors (integer numbers)
		/// to nodes, such that neighboring nodes have distinct colors. An approximate coloring
		/// algorithm is used in this routine, and is stopped when a certain percentage of nodes has
		/// been colored. The rest of the nodes are assigned distinct colors (an increasing sequence
		/// of integers numbers, starting from the last integer used previously). The last two
		/// auxiliary routines can be used to extract the resulting number of colors, their assignment
		/// and the associated reordering. The reordering is such that nodes that have been assigned
		/// the same color are reordered to be next to each other.<para/>
		/// The matrix A passed to this routine, must be stored as a general matrix and have a
		/// symmetric sparsity pattern. If the matrix is nonsymmetric the user should pass A+A^T
		/// as a parameter to this routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are 
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrSortedValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix A.</param>
		/// <param name="fractionToColor">fraction of nodes to be colored, which should be in the interval [0.0,1.0], for example 0.8 implies that 80 percent of nodes will be colored.</param>
		/// <param name="ncolors">The number of distinct colors used (at most the size of the matrix, but likely much smaller).</param>
		/// <param name="coloring">The resulting coloring permutation.</param>
		/// <param name="reordering">The resulting reordering permutation (untouched if NULL)</param>
		/// <param name="info">structure with information to be passed to the coloring.</param>
		public void Csrcolor(int m, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
			float fractionToColor, ref int ncolors, CudaDeviceVariable<int> coloring, CudaDeviceVariable<int> reordering, CudaSparseColorInfo info)
		{
			res = CudaSparseNativeMethods.cusparseScsrcolor(_handle, m, nnz, descrA.Descriptor, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				ref fractionToColor, ref ncolors, coloring.DevicePointer, reordering.DevicePointer, info.ColorInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrcolor", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the coloring of the adjacency graph associated with the matrix
		/// A stored in CSR format. The coloring is an assignment of colors (integer numbers)
		/// to nodes, such that neighboring nodes have distinct colors. An approximate coloring
		/// algorithm is used in this routine, and is stopped when a certain percentage of nodes has
		/// been colored. The rest of the nodes are assigned distinct colors (an increasing sequence
		/// of integers numbers, starting from the last integer used previously). The last two
		/// auxiliary routines can be used to extract the resulting number of colors, their assignment
		/// and the associated reordering. The reordering is such that nodes that have been assigned
		/// the same color are reordered to be next to each other.<para/>
		/// The matrix A passed to this routine, must be stored as a general matrix and have a
		/// symmetric sparsity pattern. If the matrix is nonsymmetric the user should pass A+A^T
		/// as a parameter to this routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are 
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrSortedValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix A.</param>
		/// <param name="fractionToColor">fraction of nodes to be colored, which should be in the interval [0.0,1.0], for example 0.8 implies that 80 percent of nodes will be colored.</param>
		/// <param name="ncolors">The number of distinct colors used (at most the size of the matrix, but likely much smaller).</param>
		/// <param name="coloring">The resulting coloring permutation.</param>
		/// <param name="reordering">The resulting reordering permutation (untouched if NULL)</param>
		/// <param name="info">structure with information to be passed to the coloring.</param>
		public void Csrcolor(int m, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
			double fractionToColor, ref int ncolors, CudaDeviceVariable<int> coloring, CudaDeviceVariable<int> reordering, CudaSparseColorInfo info)
		{
			res = CudaSparseNativeMethods.cusparseDcsrcolor(_handle, m, nnz, descrA.Descriptor, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				ref fractionToColor, ref ncolors, coloring.DevicePointer, reordering.DevicePointer, info.ColorInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrcolor", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the coloring of the adjacency graph associated with the matrix
		/// A stored in CSR format. The coloring is an assignment of colors (integer numbers)
		/// to nodes, such that neighboring nodes have distinct colors. An approximate coloring
		/// algorithm is used in this routine, and is stopped when a certain percentage of nodes has
		/// been colored. The rest of the nodes are assigned distinct colors (an increasing sequence
		/// of integers numbers, starting from the last integer used previously). The last two
		/// auxiliary routines can be used to extract the resulting number of colors, their assignment
		/// and the associated reordering. The reordering is such that nodes that have been assigned
		/// the same color are reordered to be next to each other.<para/>
		/// The matrix A passed to this routine, must be stored as a general matrix and have a
		/// symmetric sparsity pattern. If the matrix is nonsymmetric the user should pass A+A^T
		/// as a parameter to this routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are 
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrSortedValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix A.</param>
		/// <param name="fractionToColor">fraction of nodes to be colored, which should be in the interval [0.0,1.0], for example 0.8 implies that 80 percent of nodes will be colored.</param>
		/// <param name="ncolors">The number of distinct colors used (at most the size of the matrix, but likely much smaller).</param>
		/// <param name="coloring">The resulting coloring permutation.</param>
		/// <param name="reordering">The resulting reordering permutation (untouched if NULL)</param>
		/// <param name="info">structure with information to be passed to the coloring.</param>
		public void Csrcolor(int m, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
			float fractionToColor, ref int ncolors, CudaDeviceVariable<int> coloring, CudaDeviceVariable<int> reordering, CudaSparseColorInfo info)
		{
			res = CudaSparseNativeMethods.cusparseCcsrcolor(_handle, m, nnz, descrA.Descriptor, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				ref fractionToColor, ref ncolors, coloring.DevicePointer, reordering.DevicePointer, info.ColorInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrcolor", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the coloring of the adjacency graph associated with the matrix
		/// A stored in CSR format. The coloring is an assignment of colors (integer numbers)
		/// to nodes, such that neighboring nodes have distinct colors. An approximate coloring
		/// algorithm is used in this routine, and is stopped when a certain percentage of nodes has
		/// been colored. The rest of the nodes are assigned distinct colors (an increasing sequence
		/// of integers numbers, starting from the last integer used previously). The last two
		/// auxiliary routines can be used to extract the resulting number of colors, their assignment
		/// and the associated reordering. The reordering is such that nodes that have been assigned
		/// the same color are reordered to be next to each other.<para/>
		/// The matrix A passed to this routine, must be stored as a general matrix and have a
		/// symmetric sparsity pattern. If the matrix is nonsymmetric the user should pass A+A^T
		/// as a parameter to this routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are 
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrSortedValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix A.</param>
		/// <param name="fractionToColor">fraction of nodes to be colored, which should be in the interval [0.0,1.0], for example 0.8 implies that 80 percent of nodes will be colored.</param>
		/// <param name="ncolors">The number of distinct colors used (at most the size of the matrix, but likely much smaller).</param>
		/// <param name="coloring">The resulting coloring permutation.</param>
		/// <param name="reordering">The resulting reordering permutation (untouched if NULL)</param>
		/// <param name="info">structure with information to be passed to the coloring.</param>
		public void Csrcolor(int m, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
			double fractionToColor, ref int ncolors, CudaDeviceVariable<int> coloring, CudaDeviceVariable<int> reordering, CudaSparseColorInfo info)
		{
			res = CudaSparseNativeMethods.cusparseZcsrcolor(_handle, m, nnz, descrA.Descriptor, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				ref fractionToColor, ref ncolors, coloring.DevicePointer, reordering.DevicePointer, info.ColorInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrcolor", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the coloring of the adjacency graph associated with the matrix
		/// A stored in CSR format. The coloring is an assignment of colors (integer numbers)
		/// to nodes, such that neighboring nodes have distinct colors. An approximate coloring
		/// algorithm is used in this routine, and is stopped when a certain percentage of nodes has
		/// been colored. The rest of the nodes are assigned distinct colors (an increasing sequence
		/// of integers numbers, starting from the last integer used previously). The last two
		/// auxiliary routines can be used to extract the resulting number of colors, their assignment
		/// and the associated reordering. The reordering is such that nodes that have been assigned
		/// the same color are reordered to be next to each other.<para/>
		/// The matrix A passed to this routine, must be stored as a general matrix and have a
		/// symmetric sparsity pattern. If the matrix is nonsymmetric the user should pass A+A^T
		/// as a parameter to this routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are 
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrSortedValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix A.</param>
		/// <param name="fractionToColor">fraction of nodes to be colored, which should be in the interval [0.0,1.0], for example 0.8 implies that 80 percent of nodes will be colored.</param>
		/// <param name="ncolors">The number of distinct colors used (at most the size of the matrix, but likely much smaller).</param>
		/// <param name="coloring">The resulting coloring permutation.</param>
		/// <param name="reordering">The resulting reordering permutation (untouched if NULL)</param>
		/// <param name="info">structure with information to be passed to the coloring.</param>
		public void Csrcolor(int m, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
			CudaDeviceVariable<float> fractionToColor, CudaDeviceVariable<int> ncolors, CudaDeviceVariable<int> coloring, CudaDeviceVariable<int> reordering, CudaSparseColorInfo info)
		{
			res = CudaSparseNativeMethods.cusparseScsrcolor(_handle, m, nnz, descrA.Descriptor, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				fractionToColor.DevicePointer, ncolors.DevicePointer, coloring.DevicePointer, reordering.DevicePointer, info.ColorInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsrcolor", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the coloring of the adjacency graph associated with the matrix
		/// A stored in CSR format. The coloring is an assignment of colors (integer numbers)
		/// to nodes, such that neighboring nodes have distinct colors. An approximate coloring
		/// algorithm is used in this routine, and is stopped when a certain percentage of nodes has
		/// been colored. The rest of the nodes are assigned distinct colors (an increasing sequence
		/// of integers numbers, starting from the last integer used previously). The last two
		/// auxiliary routines can be used to extract the resulting number of colors, their assignment
		/// and the associated reordering. The reordering is such that nodes that have been assigned
		/// the same color are reordered to be next to each other.<para/>
		/// The matrix A passed to this routine, must be stored as a general matrix and have a
		/// symmetric sparsity pattern. If the matrix is nonsymmetric the user should pass A+A^T
		/// as a parameter to this routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are 
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrSortedValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix A.</param>
		/// <param name="fractionToColor">fraction of nodes to be colored, which should be in the interval [0.0,1.0], for example 0.8 implies that 80 percent of nodes will be colored.</param>
		/// <param name="ncolors">The number of distinct colors used (at most the size of the matrix, but likely much smaller).</param>
		/// <param name="coloring">The resulting coloring permutation.</param>
		/// <param name="reordering">The resulting reordering permutation (untouched if NULL)</param>
		/// <param name="info">structure with information to be passed to the coloring.</param>
		public void Csrcolor(int m, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
			CudaDeviceVariable<double> fractionToColor, CudaDeviceVariable<int> ncolors, CudaDeviceVariable<int> coloring, CudaDeviceVariable<int> reordering, CudaSparseColorInfo info)
		{
			res = CudaSparseNativeMethods.cusparseDcsrcolor(_handle, m, nnz, descrA.Descriptor, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				fractionToColor.DevicePointer, ncolors.DevicePointer, coloring.DevicePointer, reordering.DevicePointer, info.ColorInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsrcolor", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the coloring of the adjacency graph associated with the matrix
		/// A stored in CSR format. The coloring is an assignment of colors (integer numbers)
		/// to nodes, such that neighboring nodes have distinct colors. An approximate coloring
		/// algorithm is used in this routine, and is stopped when a certain percentage of nodes has
		/// been colored. The rest of the nodes are assigned distinct colors (an increasing sequence
		/// of integers numbers, starting from the last integer used previously). The last two
		/// auxiliary routines can be used to extract the resulting number of colors, their assignment
		/// and the associated reordering. The reordering is such that nodes that have been assigned
		/// the same color are reordered to be next to each other.<para/>
		/// The matrix A passed to this routine, must be stored as a general matrix and have a
		/// symmetric sparsity pattern. If the matrix is nonsymmetric the user should pass A+A^T
		/// as a parameter to this routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are 
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrSortedValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix A.</param>
		/// <param name="fractionToColor">fraction of nodes to be colored, which should be in the interval [0.0,1.0], for example 0.8 implies that 80 percent of nodes will be colored.</param>
		/// <param name="ncolors">The number of distinct colors used (at most the size of the matrix, but likely much smaller).</param>
		/// <param name="coloring">The resulting coloring permutation.</param>
		/// <param name="reordering">The resulting reordering permutation (untouched if NULL)</param>
		/// <param name="info">structure with information to be passed to the coloring.</param>
		public void Csrcolor(int m, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
			CudaDeviceVariable<float> fractionToColor, CudaDeviceVariable<int> ncolors, CudaDeviceVariable<int> coloring, CudaDeviceVariable<int> reordering, CudaSparseColorInfo info)
		{
			res = CudaSparseNativeMethods.cusparseCcsrcolor(_handle, m, nnz, descrA.Descriptor, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				fractionToColor.DevicePointer, ncolors.DevicePointer, coloring.DevicePointer, reordering.DevicePointer, info.ColorInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsrcolor", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the coloring of the adjacency graph associated with the matrix
		/// A stored in CSR format. The coloring is an assignment of colors (integer numbers)
		/// to nodes, such that neighboring nodes have distinct colors. An approximate coloring
		/// algorithm is used in this routine, and is stopped when a certain percentage of nodes has
		/// been colored. The rest of the nodes are assigned distinct colors (an increasing sequence
		/// of integers numbers, starting from the last integer used previously). The last two
		/// auxiliary routines can be used to extract the resulting number of colors, their assignment
		/// and the associated reordering. The reordering is such that nodes that have been assigned
		/// the same color are reordered to be next to each other.<para/>
		/// The matrix A passed to this routine, must be stored as a general matrix and have a
		/// symmetric sparsity pattern. If the matrix is nonsymmetric the user should pass A+A^T
		/// as a parameter to this routine.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are 
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrSortedValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) nonzero elements of matrix A.</param>
		/// <param name="csrSortedRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrSortedColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix A.</param>
		/// <param name="fractionToColor">fraction of nodes to be colored, which should be in the interval [0.0,1.0], for example 0.8 implies that 80 percent of nodes will be colored.</param>
		/// <param name="ncolors">The number of distinct colors used (at most the size of the matrix, but likely much smaller).</param>
		/// <param name="coloring">The resulting coloring permutation.</param>
		/// <param name="reordering">The resulting reordering permutation (untouched if NULL)</param>
		/// <param name="info">structure with information to be passed to the coloring.</param>
		public void Csrcolor(int m, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrSortedValA, CudaDeviceVariable<int> csrSortedRowPtrA, CudaDeviceVariable<int> csrSortedColIndA,
			CudaDeviceVariable<double> fractionToColor, CudaDeviceVariable<int> ncolors, CudaDeviceVariable<int> coloring, CudaDeviceVariable<int> reordering, CudaSparseColorInfo info)
		{
			res = CudaSparseNativeMethods.cusparseZcsrcolor(_handle, m, nnz, descrA.Descriptor, csrSortedValA.DevicePointer, csrSortedRowPtrA.DevicePointer, csrSortedColIndA.DevicePointer,
				fractionToColor.DevicePointer, ncolors.DevicePointer, coloring.DevicePointer, reordering.DevicePointer, info.ColorInfo);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsrcolor", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}







		
		/* Description: This routine converts a sparse matrix in HYB storage format
		   to a sparse matrix in CSR storage format. */
		/// <summary>
		/// This function converts a sparse matrix in HYB format into a sparse matrix in CSR format.<para/>
		/// This function requires some amount of temporary storage. It is executed asynchronously
		/// with respect to the host and it may return control to the application on the host before
		/// the result is ready.
		/// </summary>
		/// <param name="descrA">the descriptor of matrix in Hyb format. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format</param>
		/// <param name="csrValA">array of nnz csrRowPtrA(m) csrRowPtrA(0) non-zero elements of matrix A</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the start of every column and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix .</param>
		public void Hyb2csr(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			res = CudaSparseNativeMethods.cusparseShyb2csr(_handle, descrA.Descriptor, hybA.HybMat, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseShyb2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in HYB format into a sparse matrix in CSR format.<para/>
		/// This function requires some amount of temporary storage. It is executed asynchronously
		/// with respect to the host and it may return control to the application on the host before
		/// the result is ready.
		/// </summary>
		/// <param name="descrA">the descriptor of matrix in Hyb format. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format</param>
		/// <param name="csrValA">array of nnz csrRowPtrA(m) csrRowPtrA(0) non-zero elements of matrix A</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the start of every column and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix .</param>
		public void Hyb2csr(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			res = CudaSparseNativeMethods.cusparseDhyb2csr(_handle, descrA.Descriptor, hybA.HybMat, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDhyb2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in HYB format into a sparse matrix in CSR format.<para/>
		/// This function requires some amount of temporary storage. It is executed asynchronously
		/// with respect to the host and it may return control to the application on the host before
		/// the result is ready.
		/// </summary>
		/// <param name="descrA">the descriptor of matrix in Hyb format. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format</param>
		/// <param name="csrValA">array of nnz csrRowPtrA(m) csrRowPtrA(0) non-zero elements of matrix A</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the start of every column and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix .</param>
		public void Hyb2csr(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			res = CudaSparseNativeMethods.cusparseChyb2csr(_handle, descrA.Descriptor, hybA.HybMat, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseChyb2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in HYB format into a sparse matrix in CSR format.<para/>
		/// This function requires some amount of temporary storage. It is executed asynchronously
		/// with respect to the host and it may return control to the application on the host before
		/// the result is ready.
		/// </summary>
		/// <param name="descrA">the descriptor of matrix in Hyb format. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.</param>
		/// <param name="hybA">the matrix A in HYB storage format</param>
		/// <param name="csrValA">array of nnz csrRowPtrA(m) csrRowPtrA(0) non-zero elements of matrix A</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the start of every column and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz csrRowPtrA(m) csrRowPtrA(0) column indices of the nonzero elements of matrix .</param>
		public void Hyb2csr(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			res = CudaSparseNativeMethods.cusparseZhyb2csr(_handle, descrA.Descriptor, hybA.HybMat, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZhyb2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}






		/// <summary/>
		public void Csc2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA,
							CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseScsc2hyb(_handle, m, n, descrA.Descriptor, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsc2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Csc2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA,
							CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseDcsc2hyb(_handle, m, n, descrA.Descriptor, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsc2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Csc2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA,
							CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseCcsc2hyb(_handle, m, n, descrA.Descriptor, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsc2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Csc2hyb(int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> cscValA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> cscColPtrA,
							CudaSparseHybMat hybA, int userEllWidth, cusparseHybPartition partitionType)
		{
			res = CudaSparseNativeMethods.cusparseZcsc2hyb(_handle, m, n, descrA.Descriptor, cscValA.DevicePointer, cscRowIndA.DevicePointer, cscColPtrA.DevicePointer, hybA.HybMat, userEllWidth, partitionType);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsc2hyb", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/* Description: This routine converts a sparse matrix in HYB storage format
		   to a sparse matrix in CSC storage format. */

		/// <summary/>
		public void Hyb2csc(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<float> cscVal, CudaDeviceVariable<int> cscRowInd, CudaDeviceVariable<int> cscColPtr)
		{
			res = CudaSparseNativeMethods.cusparseShyb2csc(_handle, descrA.Descriptor, hybA.HybMat, cscVal.DevicePointer, cscRowInd.DevicePointer, cscColPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseShyb2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Hyb2csc(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<double> cscVal, CudaDeviceVariable<int> cscRowInd, CudaDeviceVariable<int> cscColPtr)
		{
			res = CudaSparseNativeMethods.cusparseDhyb2csc(_handle, descrA.Descriptor, hybA.HybMat, cscVal.DevicePointer, cscRowInd.DevicePointer, cscColPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDhyb2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Hyb2csc(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuFloatComplex> cscVal, CudaDeviceVariable<int> cscRowInd, CudaDeviceVariable<int> cscColPtr)
		{
			res = CudaSparseNativeMethods.cusparseChyb2csc(_handle, descrA.Descriptor, hybA.HybMat, cscVal.DevicePointer, cscRowInd.DevicePointer, cscColPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseChyb2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary/>
		public void Hyb2csc(CudaSparseMatrixDescriptor descrA, CudaSparseHybMat hybA, CudaDeviceVariable<cuDoubleComplex> cscVal, CudaDeviceVariable<int> cscRowInd, CudaDeviceVariable<int> cscColPtr)
		{
			res = CudaSparseNativeMethods.cusparseZhyb2csc(_handle, descrA.Descriptor, hybA.HybMat, cscVal.DevicePointer, cscRowInd.DevicePointer, cscColPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZhyb2csc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}













		/// <summary>
		/// This function converts a sparse matrix in CSR format (that is defined by the three arrays
		/// csrValA, csrRowPtrA and csrColIndA) into a sparse matrix in BSR format (that is
		/// defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// A is m x n sparse matrix and C is (mb*blockDim) x (nb*blockDim) sparse matrix.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A. The range of blockDim is between
		/// 1 and min(m, n).</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="nnzTotalDevHostPtr"></param>
		public void Csr2bsrNnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseXcsr2bsrNnz(_handle, dirA, m, n, descrA.Descriptor, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, blockDim, descrC.Descriptor, bsrRowPtrC.DevicePointer, nnzTotalDevHostPtr.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsr2bsrNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in CSR format (that is defined by the three arrays
		/// csrValA, csrRowPtrA and csrColIndA) into a sparse matrix in BSR format (that is
		/// defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// A is m x n sparse matrix and C is (mb*blockDim) x (nb*blockDim) sparse matrix.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A. The range of blockDim is between
		/// 1 and min(m, n).</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="nnzTotalDevHostPtr"></param>
		public void Csr2bsrNnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> bsrRowPtrC, ref int nnzTotalDevHostPtr)
		{
			res = CudaSparseNativeMethods.cusparseXcsr2bsrNnz(_handle, dirA, m, n, descrA.Descriptor, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, blockDim, descrC.Descriptor, bsrRowPtrC.DevicePointer, ref nnzTotalDevHostPtr);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsr2bsrNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function converts a sparse matrix in CSR format (that is defined by the three arrays
		/// csrValA, csrRowPtrA and csrColIndA) into a sparse matrix in BSR format (that is
		/// defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// A is m x n sparse matrix and C is (mb*blockDim) x (nb*blockDim) sparse matrix.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero 
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A. The range of blockDim is between
		/// 1 and min(m, n).</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="bsrValC">array of nnzb*blockDim² non-zero elements of matrix C.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndC">integer array of nnzb column indices of the non-zero blocks of matrix C.</param>
		public void Csr2bsr(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> bsrValC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseScsr2bsr(_handle, dirA, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, blockDim, descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsr2bsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in CSR format (that is defined by the three arrays
		/// csrValA, csrRowPtrA and csrColIndA) into a sparse matrix in BSR format (that is
		/// defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// A is m x n sparse matrix and C is (mb*blockDim) x (nb*blockDim) sparse matrix.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero 
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A. The range of blockDim is between
		/// 1 and min(m, n).</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="bsrValC">array of nnzb*blockDim² non-zero elements of matrix C.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndC">integer array of nnzb column indices of the non-zero blocks of matrix C.</param>
		public void Csr2bsr(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> bsrValC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseDcsr2bsr(_handle, dirA, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, blockDim, descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsr2bsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in CSR format (that is defined by the three arrays
		/// csrValA, csrRowPtrA and csrColIndA) into a sparse matrix in BSR format (that is
		/// defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// A is m x n sparse matrix and C is (mb*blockDim) x (nb*blockDim) sparse matrix.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero 
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A. The range of blockDim is between
		/// 1 and min(m, n).</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="bsrValC">array of nnzb*blockDim² non-zero elements of matrix C.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndC">integer array of nnzb column indices of the non-zero blocks of matrix C.</param>
		public void Csr2bsr(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> bsrValC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseCcsr2bsr(_handle, dirA, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, blockDim, descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsr2bsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in CSR format (that is defined by the three arrays
		/// csrValA, csrRowPtrA and csrColIndA) into a sparse matrix in BSR format (that is
		/// defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// A is m x n sparse matrix and C is (mb*blockDim) x (nb*blockDim) sparse matrix.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="csrValA">array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) non-zero 
		/// elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz (= csrRowPtrA(m) - csrRowPtrA(0)) column
		/// indices of the non-zero elements of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A. The range of blockDim is between
		/// 1 and min(m, n).</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="bsrValC">array of nnzb*blockDim² non-zero elements of matrix C.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndC">integer array of nnzb column indices of the non-zero blocks of matrix C.</param>
		public void Csr2bsr(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> bsrValC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseZcsr2bsr(_handle, dirA, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, blockDim, descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsr2bsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function converts a sparse matrix in BSR format (that is defined by the three arrays
		/// bsrValA, bsrRowPtrA and bsrColIndA) into a sparse matrix in CSR format (that is
		/// defined by arrays csrValC, csrRowPtrC, and csrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A. The number of rows of sparse matrix C is m(= mb*blockDim).</param>
		/// <param name="nb">number of block columns of sparse matrix A. The number of columns of sparse matrix C is n(= nb*blockDim).</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="bsrValA">array of nnzb*blockDim² non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the non-zero blocks of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="csrValC">array of nnz (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the non-zero elements of matrix C.</param>
		public void Bsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseSbsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in BSR format (that is defined by the three arrays
		/// bsrValA, bsrRowPtrA and bsrColIndA) into a sparse matrix in CSR format (that is
		/// defined by arrays csrValC, csrRowPtrC, and csrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A. The number of rows of sparse matrix C is m(= mb*blockDim).</param>
		/// <param name="nb">number of block columns of sparse matrix A. The number of columns of sparse matrix C is n(= nb*blockDim).</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="bsrValA">array of nnzb*blockDim² non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the non-zero blocks of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="csrValC">array of nnz (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the non-zero elements of matrix C.</param>
		public void Bsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseDbsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in BSR format (that is defined by the three arrays
		/// bsrValA, bsrRowPtrA and bsrColIndA) into a sparse matrix in CSR format (that is
		/// defined by arrays csrValC, csrRowPtrC, and csrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A. The number of rows of sparse matrix C is m(= mb*blockDim).</param>
		/// <param name="nb">number of block columns of sparse matrix A. The number of columns of sparse matrix C is n(= nb*blockDim).</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="bsrValA">array of nnzb*blockDim² non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the non-zero blocks of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="csrValC">array of nnz (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the non-zero elements of matrix C.</param>
		public void Bsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseCbsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function converts a sparse matrix in BSR format (that is defined by the three arrays
		/// bsrValA, bsrRowPtrA and bsrColIndA) into a sparse matrix in CSR format (that is
		/// defined by arrays csrValC, csrRowPtrC, and csrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A. The number of rows of sparse matrix C is m(= mb*blockDim).</param>
		/// <param name="nb">number of block columns of sparse matrix A. The number of columns of sparse matrix C is n(= nb*blockDim).</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="bsrValA">array of nnzb*blockDim² non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the non-zero blocks of matrix A.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="descrC">the descriptor of matrix C.</param>
		/// <param name="csrValC">array of nnz (= csrRowPtrC(m) - csrRowPtrC(0)) non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m + 1 elements that contains the start of every row
		/// and the end of the last row plus one.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the non-zero elements of matrix C.</param>
		public void Bsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseZbsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, descrC.Descriptor, csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		#region Removed in Cuda 5.5 production release, present in pre-release, again in Cuda 6
		/// <summary>
		/// This function returns size of buffer used in computing gebsr2gebsc.
		/// </summary>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="bsrVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last
		/// block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <returns>number of bytes of the buffer used in the gebsr2gebsc.</returns>
		public SizeT Gebsr2gebscBufferSize(int mb, int nb, CudaDeviceVariable<float> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd, int rowBlockDim, int colBlockDim)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseSgebsr2gebsc_bufferSizeExt(_handle, mb, nb, (int)bsrColInd.Size, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, rowBlockDim, colBlockDim, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgebsr2gebsc_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing gebsr2gebsc.
		/// </summary>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="bsrVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last
		/// block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <returns>number of bytes of the buffer used in the gebsr2gebsc.</returns>
		public SizeT Gebsr2gebscBufferSize(int mb, int nb, CudaDeviceVariable<double> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd, int rowBlockDim, int colBlockDim)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDgebsr2gebsc_bufferSizeExt(_handle, mb, nb, (int)bsrColInd.Size, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, rowBlockDim, colBlockDim, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgebsr2gebsc_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing gebsr2gebsc.
		/// </summary>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="bsrVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last
		/// block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <returns>number of bytes of the buffer used in the gebsr2gebsc.</returns>
		public SizeT Gebsr2gebscBufferSize(int mb, int nb, CudaDeviceVariable<cuFloatComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd, int rowBlockDim, int colBlockDim)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCgebsr2gebsc_bufferSizeExt(_handle, mb, nb, (int)bsrColInd.Size, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, rowBlockDim, colBlockDim, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgebsr2gebsc_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing gebsr2gebsc.
		/// </summary>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="bsrVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last
		/// block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <returns>number of bytes of the buffer used in the gebsr2gebsc.</returns>
		public SizeT Gebsr2gebscBufferSize(int mb, int nb, CudaDeviceVariable<cuDoubleComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd, int rowBlockDim, int colBlockDim)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZgebsr2gebsc_bufferSizeExt(_handle, mb, nb, (int)bsrColInd.Size, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, rowBlockDim, colBlockDim, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgebsr2gebsc_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}



		/// <summary>
		/// This function returns the size of the buffer used in computing csr2gebsrNnz and csr2gebsr.
		/// </summary>
		/// <param name="dir">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A.</param>
		/// <param name="csrColInd">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <returns>number of bytes of the buffer used in csr2gebsrNnz() and csr2gebsr().</returns>
		public SizeT Csr2gebsrBufferSize(cusparseDirection dir, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrVal, 
			CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, int rowBlockDim, int colBlockDim)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseScsr2gebsr_bufferSizeExt(_handle, dir, m, n, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, rowBlockDim, colBlockDim, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsr2gebsr_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}



		/// <summary>
		/// This function returns the size of the buffer used in computing csr2gebsrNnz and csr2gebsr.
		/// </summary>
		/// <param name="dir">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A.</param>
		/// <param name="csrColInd">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <returns>number of bytes of the buffer used in csr2gebsrNnz() and csr2gebsr().</returns>
		public SizeT Csr2gebsrBufferSize(cusparseDirection dir, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrVal,
			CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, int rowBlockDim, int colBlockDim)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDcsr2gebsr_bufferSizeExt(_handle, dir, m, n, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, rowBlockDim, colBlockDim, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsr2gebsr_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}



		/// <summary>
		/// This function returns the size of the buffer used in computing csr2gebsrNnz and csr2gebsr.
		/// </summary>
		/// <param name="dir">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A.</param>
		/// <param name="csrColInd">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <returns>number of bytes of the buffer used in csr2gebsrNnz() and csr2gebsr().</returns>
		public SizeT Csr2gebsrBufferSize(cusparseDirection dir, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrVal,
			CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, int rowBlockDim, int colBlockDim)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCcsr2gebsr_bufferSizeExt(_handle, dir, m, n, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, rowBlockDim, colBlockDim, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsr2gebsr_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}



		/// <summary>
		/// This function returns the size of the buffer used in computing csr2gebsrNnz and csr2gebsr.
		/// </summary>
		/// <param name="dir">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A.</param>
		/// <param name="csrColInd">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <returns>number of bytes of the buffer used in csr2gebsrNnz() and csr2gebsr().</returns>
		public SizeT Csr2gebsrBufferSize(cusparseDirection dir, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrVal,
			CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, int rowBlockDim, int colBlockDim)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZcsr2gebsr_bufferSizeExt(_handle, dir, m, n, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, rowBlockDim, colBlockDim, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsr2gebsr_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		

		/// <summary>
		/// This function returns size of buffer used in computing gebsr2gebsrNnz and gebsr2gebsr.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDimA*colBlockDimA non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C.</param>
		/// <param name="colBlockDimC">number of columns within a block of C.</param>
		/// <returns>number of bytes of the buffer used in csr2gebsrNnz() and csr2gebsr().</returns>
		public SizeT Gebsr2gebsrBufferSize(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA,
			CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int rowBlockDimA, int colBlockDimA, int rowBlockDimC, int colBlockDimC)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseSgebsr2gebsr_bufferSizeExt(_handle, dirA, mb, nb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA, rowBlockDimC, colBlockDimC, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgebsr2gebsr_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing gebsr2gebsrNnz and gebsr2gebsr.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDimA*colBlockDimA non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C.</param>
		/// <param name="colBlockDimC">number of columns within a block of C.</param>
		/// <returns>number of bytes of the buffer used in csr2gebsrNnz() and csr2gebsr().</returns>
		public SizeT Gebsr2gebsrBufferSize(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA,
			CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int rowBlockDimA, int colBlockDimA, int rowBlockDimC, int colBlockDimC)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDgebsr2gebsr_bufferSizeExt(_handle, dirA, mb, nb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA, rowBlockDimC, colBlockDimC, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgebsr2gebsr_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing gebsr2gebsrNnz and gebsr2gebsr.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDimA*colBlockDimA non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C.</param>
		/// <param name="colBlockDimC">number of columns within a block of C.</param>
		/// <returns>number of bytes of the buffer used in csr2gebsrNnz() and csr2gebsr().</returns>
		public SizeT Gebsr2gebsrBufferSize(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA,
			CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int rowBlockDimA, int colBlockDimA, int rowBlockDimC, int colBlockDimC)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCgebsr2gebsr_bufferSizeExt(_handle, dirA, mb, nb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA, rowBlockDimC, colBlockDimC, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgebsr2gebsr_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}
		/// <summary>
		/// This function returns size of buffer used in computing gebsr2gebsrNnz and gebsr2gebsr.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is CUSPARSE_MATRIX_TYPE_GENERAL.
		/// Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDimA*colBlockDimA non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C.</param>
		/// <param name="colBlockDimC">number of columns within a block of C.</param>
		/// <returns>number of bytes of the buffer used in csr2gebsrNnz() and csr2gebsr().</returns>
		public SizeT Gebsr2gebsrBufferSize(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA,
			CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int rowBlockDimA, int colBlockDimA, int rowBlockDimC, int colBlockDimC)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZgebsr2gebsr_bufferSizeExt(_handle, dirA, mb, nb, (int)bsrColIndA.Size, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA, rowBlockDimC, colBlockDimC, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgebsr2gebsr_bufferSizeExt(", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}


		/// <summary>
		/// This function can be seen as the same as csr2csc when regarding each block of size
		/// rowBlockDim*colBlockDim as a scalar.<para/>
		/// This sparsity pattern of result matrix can also be seen as the transpose of the original
		/// sparse matrix but memory layout of a block does not change.<para/>
		/// The user must know the size of buffer required by gebsr2gebsc by calling
		/// gebsr2gebsc_bufferSizeExt, allocate the buffer and pass the buffer pointer to gebsr2gebsc.
		/// </summary>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb+1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="bscVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A. It is only filled-in if copyValues is set to CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="bscRowInd">integer array of nnzb row indices of the non-zero blocks of matrix A</param>
		/// <param name="bscColPtr">integer array of nb+1 elements that contains the start of every block column and the end of the last block column plus one.</param>
		/// <param name="copyValues">CUSPARSE_ACTION_SYMBOLIC or CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="baseIdx">CUSPARSE_INDEX_BASE_ZERO or CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsc_bufferSizeExt.</param>
		public void Gebsr2gebsc(int mb, int nb, int nnzb, CudaDeviceVariable<float> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
								int rowBlockDim, int colBlockDim, CudaDeviceVariable<float> bscVal, CudaDeviceVariable<int> bscRowInd, CudaDeviceVariable<int> bscColPtr,
								cusparseAction copyValues, cusparseIndexBase baseIdx, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseSgebsr2gebsc(_handle, mb, nb, nnzb, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, rowBlockDim, colBlockDim, bscVal.DevicePointer,
				bscRowInd.DevicePointer, bscColPtr.DevicePointer, copyValues, baseIdx, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgebsr2gebsc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function can be seen as the same as csr2csc when regarding each block of size
		/// rowBlockDim*colBlockDim as a scalar.<para/>
		/// This sparsity pattern of result matrix can also be seen as the transpose of the original
		/// sparse matrix but memory layout of a block does not change.<para/>
		/// The user must know the size of buffer required by gebsr2gebsc by calling
		/// gebsr2gebsc_bufferSizeExt, allocate the buffer and pass the buffer pointer to gebsr2gebsc.
		/// </summary>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb+1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="bscVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A. It is only filled-in if copyValues is set to CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="bscRowInd">integer array of nnzb row indices of the non-zero blocks of matrix A</param>
		/// <param name="bscColPtr">integer array of nb+1 elements that contains the start of every block column and the end of the last block column plus one.</param>
		/// <param name="copyValues">CUSPARSE_ACTION_SYMBOLIC or CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="baseIdx">CUSPARSE_INDEX_BASE_ZERO or CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsc_bufferSizeExt.</param>
		public void Gebsr2gebsc(int mb, int nb, int nnzb, CudaDeviceVariable<double> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
								int rowBlockDim, int colBlockDim, CudaDeviceVariable<double> bscVal, CudaDeviceVariable<int> bscRowInd, CudaDeviceVariable<int> bscColPtr,
								cusparseAction copyValues, cusparseIndexBase baseIdx, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDgebsr2gebsc(_handle, mb, nb, nnzb, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, rowBlockDim, colBlockDim, bscVal.DevicePointer,
				bscRowInd.DevicePointer, bscColPtr.DevicePointer, copyValues, baseIdx, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgebsr2gebsc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This function can be seen as the same as csr2csc when regarding each block of size
		/// rowBlockDim*colBlockDim as a scalar.<para/>
		/// This sparsity pattern of result matrix can also be seen as the transpose of the original
		/// sparse matrix but memory layout of a block does not change.<para/>
		/// The user must know the size of buffer required by gebsr2gebsc by calling
		/// gebsr2gebsc_bufferSizeExt, allocate the buffer and pass the buffer pointer to gebsr2gebsc.
		/// </summary>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb+1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="bscVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A. It is only filled-in if copyValues is set to CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="bscRowInd">integer array of nnzb row indices of the non-zero blocks of matrix A</param>
		/// <param name="bscColPtr">integer array of nb+1 elements that contains the start of every block column and the end of the last block column plus one.</param>
		/// <param name="copyValues">CUSPARSE_ACTION_SYMBOLIC or CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="baseIdx">CUSPARSE_INDEX_BASE_ZERO or CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsc_bufferSizeExt.</param>
		public void Gebsr2gebsc(int mb, int nb, int nnzb, CudaDeviceVariable<cuFloatComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
								int rowBlockDim, int colBlockDim, CudaDeviceVariable<cuFloatComplex> bscVal, CudaDeviceVariable<int> bscRowInd, CudaDeviceVariable<int> bscColPtr,
								cusparseAction copyValues, cusparseIndexBase baseIdx, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCgebsr2gebsc(_handle, mb, nb, nnzb, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, rowBlockDim, colBlockDim, bscVal.DevicePointer,
				bscRowInd.DevicePointer, bscColPtr.DevicePointer, copyValues, baseIdx, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgebsr2gebsc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		/// <summary>
		/// This function can be seen as the same as csr2csc when regarding each block of size
		/// rowBlockDim*colBlockDim as a scalar.<para/>
		/// This sparsity pattern of result matrix can also be seen as the transpose of the original
		/// sparse matrix but memory layout of a block does not change.<para/>
		/// The user must know the size of buffer required by gebsr2gebsc by calling
		/// gebsr2gebsc_bufferSizeExt, allocate the buffer and pass the buffer pointer to gebsr2gebsc.
		/// </summary>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtr">integer array of mb+1 elements that contains the start of every block row and the end of the last block row plus one.</param>
		/// <param name="bsrColInd">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="bscVal">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A. It is only filled-in if copyValues is set to CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="bscRowInd">integer array of nnzb row indices of the non-zero blocks of matrix A</param>
		/// <param name="bscColPtr">integer array of nb+1 elements that contains the start of every block column and the end of the last block column plus one.</param>
		/// <param name="copyValues">CUSPARSE_ACTION_SYMBOLIC or CUSPARSE_ACTION_NUMERIC.</param>
		/// <param name="baseIdx">CUSPARSE_INDEX_BASE_ZERO or CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsc_bufferSizeExt.</param>
		public void Gebsr2gebsc(int mb, int nb, int nnzb, CudaDeviceVariable<cuDoubleComplex> bsrVal, CudaDeviceVariable<int> bsrRowPtr, CudaDeviceVariable<int> bsrColInd,
								int rowBlockDim, int colBlockDim, CudaDeviceVariable<cuDoubleComplex> bscVal, CudaDeviceVariable<int> bscRowInd, CudaDeviceVariable<int> bscColPtr,
								cusparseAction copyValues, cusparseIndexBase baseIdx, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZgebsr2gebsc(_handle, mb, nb, nnzb, bsrVal.DevicePointer, bsrRowPtr.DevicePointer, bsrColInd.DevicePointer, rowBlockDim, colBlockDim, bscVal.DevicePointer,
				bscRowInd.DevicePointer, bscColPtr.DevicePointer, copyValues, baseIdx, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgebsr2gebsc", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in CSR format
		/// (that is defined by arrays csrValC, csrRowPtrC, and csrColIndC).<para/>
		/// Let m(=mb*rowBlockDim) be number of rows of A and n(=nb*colBlockDim) be
		/// number of columns of A, then A and C are m*n sparse matrices. General BSR format of
		/// A contains nnzb(=bsrRowPtrA[mb] - bsrRowPtrA[0]) non-zero blocks whereas
		/// sparse matrix A contains nnz(=nnzb*rowBlockDim*colBockDim) elements. The user
		/// must allocate enough space for arrays csrRowPtrC, csrColIndC and csrValC. The
		/// requirements are<para/>
		/// csrRowPtrC of m+1 elements,<para/>
		/// csrValC of nnz elements, and<para/>
		/// csrColIndC of nnz elements.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrC">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix C.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the nonzero elements of matrix C.</param>
		public void Gebsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA,
							  int rowBlockDim, int colBlockDim, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseXgebsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDim, colBlockDim, descrC.Descriptor,
				csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXgebsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in CSR format
		/// (that is defined by arrays csrValC, csrRowPtrC, and csrColIndC).<para/>
		/// Let m(=mb*rowBlockDim) be number of rows of A and n(=nb*colBlockDim) be
		/// number of columns of A, then A and C are m*n sparse matrices. General BSR format of
		/// A contains nnzb(=bsrRowPtrA[mb] - bsrRowPtrA[0]) non-zero blocks whereas
		/// sparse matrix A contains nnz(=nnzb*rowBlockDim*colBockDim) elements. The user
		/// must allocate enough space for arrays csrRowPtrC, csrColIndC and csrValC. The
		/// requirements are<para/>
		/// csrRowPtrC of m+1 elements,<para/>
		/// csrValC of nnz elements, and<para/>
		/// csrColIndC of nnz elements.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValC">array of nnz non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix C.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the nonzero elements of matrix C.</param>
		public void Gebsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA,
							  CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int rowBlockDim, int colBlockDim,
							  CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseSgebsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDim, colBlockDim, descrC.Descriptor,
				csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgebsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}




		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in CSR format
		/// (that is defined by arrays csrValC, csrRowPtrC, and csrColIndC).<para/>
		/// Let m(=mb*rowBlockDim) be number of rows of A and n(=nb*colBlockDim) be
		/// number of columns of A, then A and C are m*n sparse matrices. General BSR format of
		/// A contains nnzb(=bsrRowPtrA[mb] - bsrRowPtrA[0]) non-zero blocks whereas
		/// sparse matrix A contains nnz(=nnzb*rowBlockDim*colBockDim) elements. The user
		/// must allocate enough space for arrays csrRowPtrC, csrColIndC and csrValC. The
		/// requirements are<para/>
		/// csrRowPtrC of m+1 elements,<para/>
		/// csrValC of nnz elements, and<para/>
		/// csrColIndC of nnz elements.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValC">array of nnz non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix C.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the nonzero elements of matrix C.</param>
		public void Gebsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA,
							  CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int rowBlockDim, int colBlockDim,
							  CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseDgebsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDim, colBlockDim, descrC.Descriptor,
				csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgebsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}




		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in CSR format
		/// (that is defined by arrays csrValC, csrRowPtrC, and csrColIndC).<para/>
		/// Let m(=mb*rowBlockDim) be number of rows of A and n(=nb*colBlockDim) be
		/// number of columns of A, then A and C are m*n sparse matrices. General BSR format of
		/// A contains nnzb(=bsrRowPtrA[mb] - bsrRowPtrA[0]) non-zero blocks whereas
		/// sparse matrix A contains nnz(=nnzb*rowBlockDim*colBockDim) elements. The user
		/// must allocate enough space for arrays csrRowPtrC, csrColIndC and csrValC. The
		/// requirements are<para/>
		/// csrRowPtrC of m+1 elements,<para/>
		/// csrValC of nnz elements, and<para/>
		/// csrColIndC of nnz elements.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValC">array of nnz non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix C.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the nonzero elements of matrix C.</param>
		public void Gebsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA,
							  CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int rowBlockDim, int colBlockDim,
							  CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseCgebsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDim, colBlockDim, descrC.Descriptor,
				csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgebsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}




		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in CSR format
		/// (that is defined by arrays csrValC, csrRowPtrC, and csrColIndC).<para/>
		/// Let m(=mb*rowBlockDim) be number of rows of A and n(=nb*colBlockDim) be
		/// number of columns of A, then A and C are m*n sparse matrices. General BSR format of
		/// A contains nnzb(=bsrRowPtrA[mb] - bsrRowPtrA[0]) non-zero blocks whereas
		/// sparse matrix A contains nnz(=nnzb*rowBlockDim*colBockDim) elements. The user
		/// must allocate enough space for arrays csrRowPtrC, csrColIndC and csrValC. The
		/// requirements are<para/>
		/// csrRowPtrC of m+1 elements,<para/>
		/// csrValC of nnz elements, and<para/>
		/// csrColIndC of nnz elements.
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDim*colBlockDim non-zero elements of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDim">number of rows within a block of A.</param>
		/// <param name="colBlockDim">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrValC">array of nnz non-zero elements of matrix C.</param>
		/// <param name="csrRowPtrC">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix C.</param>
		/// <param name="csrColIndC">integer array of nnz column indices of the nonzero elements of matrix C.</param>
		public void Gebsr2csr(cusparseDirection dirA, int mb, int nb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA,
							  CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int rowBlockDim, int colBlockDim,
							  CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> csrValC, CudaDeviceVariable<int> csrRowPtrC, CudaDeviceVariable<int> csrColIndC)
		{
			res = CudaSparseNativeMethods.cusparseZgebsr2csr(_handle, dirA, mb, nb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDim, colBlockDim, descrC.Descriptor,
				csrValC.DevicePointer, csrRowPtrC.DevicePointer, csrColIndC.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgebsr2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix A in CSR format (that is defined by arrays
		/// csrValA, csrRowPtrA, and csrColIndA) into a sparse matrix C in general BSR format
		/// (that is defined by the three arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A</param>
		/// <param name="csrColIndA">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <param name="nnzTotalDevHostPtr">total number of nonzero blocks of matrix C. <para/>
		/// Pointer nnzTotalDevHostPtr can point to a device memory or host memory.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by csr2gebsr_bufferSizeExt().</param>
		public void Csr2gebsrNnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA,
								 CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> bsrRowPtrC, int rowBlockDim, int colBlockDim, CudaDeviceVariable<int> nnzTotalDevHostPtr, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseXcsr2gebsrNnz(_handle, dirA, m, n, descrA.Descriptor, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrC.Descriptor, bsrRowPtrC.DevicePointer, rowBlockDim, colBlockDim, nnzTotalDevHostPtr.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsr2gebsrNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix A in CSR format (that is defined by arrays
		/// csrValA, csrRowPtrA, and csrColIndA) into a sparse matrix C in general BSR format
		/// (that is defined by the three arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A</param>
		/// <param name="csrColIndA">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <param name="nnzTotalDevHostPtr">total number of nonzero blocks of matrix C. <para/>
		/// Pointer nnzTotalDevHostPtr can point to a device memory or host memory.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by csr2gebsr_bufferSizeExt().</param>
		public void Csr2gebsrNnz(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA,
								 CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> bsrRowPtrC, int rowBlockDim, int colBlockDim, ref int nnzTotalDevHostPtr, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseXcsr2gebsrNnz(_handle, dirA, m, n, descrA.Descriptor, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrC.Descriptor, bsrRowPtrC.DevicePointer, rowBlockDim, colBlockDim, ref nnzTotalDevHostPtr, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsr2gebsrNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix A in CSR format (that is defined by arrays
		/// csrValA, csrRowPtrA, and csrColIndA) into a sparse matrix C in general BSR format
		/// (that is defined by the three arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A</param>
		/// <param name="csrColIndA">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <param name="bsrColIndC"><para/>
		/// Pointer nnzTotalDevHostPtr can point to a device memory or host memory.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by csr2gebsr_bufferSizeExt().</param>
		/// <param name="csrValA">array of nnz nonzero elements of matrix A.</param>
		/// <param name="bsrValC">array of nnzb*rowBlockDim*colBlockDim nonzero elements of matrix C.</param>
		public void Csr2gebsr(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA,
							  CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> bsrValC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC, int rowBlockDim, int colBlockDim, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseScsr2gebsr(_handle, dirA, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer, rowBlockDim, colBlockDim, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsr2gebsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix A in CSR format (that is defined by arrays
		/// csrValA, csrRowPtrA, and csrColIndA) into a sparse matrix C in general BSR format
		/// (that is defined by the three arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A</param>
		/// <param name="csrColIndA">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <param name="bsrColIndC"><para/>
		/// Pointer nnzTotalDevHostPtr can point to a device memory or host memory.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by csr2gebsr_bufferSizeExt().</param>
		/// <param name="csrValA">array of nnz nonzero elements of matrix A.</param>
		/// <param name="bsrValC">array of nnzb*rowBlockDim*colBlockDim nonzero elements of matrix C.</param>
		public void Csr2gebsr(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA,
							  CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> bsrValC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC, int rowBlockDim, int colBlockDim, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsr2gebsr(_handle, dirA, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer, rowBlockDim, colBlockDim, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsr2gebsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix A in CSR format (that is defined by arrays
		/// csrValA, csrRowPtrA, and csrColIndA) into a sparse matrix C in general BSR format
		/// (that is defined by the three arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A</param>
		/// <param name="csrColIndA">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <param name="bsrColIndC"><para/>
		/// Pointer nnzTotalDevHostPtr can point to a device memory or host memory.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by csr2gebsr_bufferSizeExt().</param>
		/// <param name="csrValA">array of nnz nonzero elements of matrix A.</param>
		/// <param name="bsrValC">array of nnzb*rowBlockDim*colBlockDim nonzero elements of matrix C.</param>
		public void Csr2gebsr(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA,
							  CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> bsrValC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC, int rowBlockDim, int colBlockDim, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsr2gebsr(_handle, dirA, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer, rowBlockDim, colBlockDim, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsr2gebsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function converts a sparse matrix A in CSR format (that is defined by arrays
		/// csrValA, csrRowPtrA, and csrColIndA) into a sparse matrix C in general BSR format
		/// (that is defined by the three arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="m">number of rows of sparse matrix A.</param>
		/// <param name="n">number of columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the
		/// start of every row and the end of the last row plus one of matrix A</param>
		/// <param name="csrColIndA">integer array of nnz column indices of the nonzero elements of matrix A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="rowBlockDim">number of rows within a block of C.</param>
		/// <param name="colBlockDim">number of columns within a block of C.</param>
		/// <param name="bsrColIndC"><para/>
		/// Pointer nnzTotalDevHostPtr can point to a device memory or host memory.</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by csr2gebsr_bufferSizeExt().</param>
		/// <param name="csrValA">array of nnz nonzero elements of matrix A.</param>
		/// <param name="bsrValC">array of nnzb*rowBlockDim*colBlockDim nonzero elements of matrix C.</param>
		public void Csr2gebsr(cusparseDirection dirA, int m, int n, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrValA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA,
							  CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> bsrValC, CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC, int rowBlockDim, int colBlockDim, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsr2gebsr(_handle, dirA, m, n, descrA.Descriptor, csrValA.DevicePointer, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer, rowBlockDim, colBlockDim, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsr2gebsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in another general
		/// BSR format (that is defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mc+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C</param>
		/// <param name="colBlockDimC">number of columns within a block of C</param>
		/// <param name="nnzTotalDevHostPtr">total number of nonzero blocks of C.<para/>
		/// nnzTotalDevHostPtr is the same as bsrRowPtrC[mc]-bsrRowPtrC[0]</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsr_bufferSizeExt.</param>
		public void Gebsr2gebsrNnz(cusparseDirection dirA, int mb, int nb, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA,
								   int rowBlockDimA, int colBlockDimA, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> bsrRowPtrC, int rowBlockDimC, int colBlockDimC, CudaDeviceVariable<int> nnzTotalDevHostPtr, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseXgebsr2gebsrNnz(_handle, dirA, mb, nb, nnzb, descrA.Descriptor, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA, descrC.Descriptor, bsrRowPtrC.DevicePointer, rowBlockDimC, colBlockDimC, nnzTotalDevHostPtr.DevicePointer, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXgebsr2gebsrNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in another general
		/// BSR format (that is defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mc+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C</param>
		/// <param name="colBlockDimC">number of columns within a block of C</param>
		/// <param name="nnzTotalDevHostPtr">total number of nonzero blocks of C.<para/>
		/// nnzTotalDevHostPtr is the same as bsrRowPtrC[mc]-bsrRowPtrC[0]</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsr_bufferSizeExt.</param>
		public void Gebsr2gebsrNnz(cusparseDirection dirA, int mb, int nb, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA,
								   int rowBlockDimA, int colBlockDimA, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<int> bsrRowPtrC, int rowBlockDimC, int colBlockDimC, ref int nnzTotalDevHostPtr, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseXgebsr2gebsrNnz(_handle, dirA, mb, nb, nnzb, descrA.Descriptor, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA, descrC.Descriptor, bsrRowPtrC.DevicePointer, rowBlockDimC, colBlockDimC, ref nnzTotalDevHostPtr, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXgebsr2gebsrNnz", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in another general
		/// BSR format (that is defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mc+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="bsrColIndC">integer array of nnzc block column indices of the non-zero blocks of matrix C.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C</param>
		/// <param name="colBlockDimC">number of columns within a block of C</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsr_bufferSizeExt.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDimA*colBlockDimA non-zero elements of matrix A.</param>
		/// <param name="bsrValC">array of nnzc*rowBlockDimC*colBlockDimC non-zero elements of matrix C.</param>
		public void Gebsr2gebsr(cusparseDirection dirA, int mb, int nb, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
								CudaDeviceVariable<int> bsrColIndA, int rowBlockDimA, int colBlockDimA, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<float> bsrValC,
								CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC, int rowBlockDimC, int colBlockDimC, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseSgebsr2gebsr(_handle, dirA, mb, nb, nnzb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA,
				descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer, rowBlockDimC, colBlockDimC, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSgebsr2gebsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in another general
		/// BSR format (that is defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mc+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="bsrColIndC">integer array of nnzc block column indices of the non-zero blocks of matrix C.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C</param>
		/// <param name="colBlockDimC">number of columns within a block of C</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsr_bufferSizeExt.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDimA*colBlockDimA non-zero elements of matrix A.</param>
		/// <param name="bsrValC">array of nnzc*rowBlockDimC*colBlockDimC non-zero elements of matrix C.</param>
		public void Gebsr2gebsr(cusparseDirection dirA, int mb, int nb, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
								CudaDeviceVariable<int> bsrColIndA, int rowBlockDimA, int colBlockDimA, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<double> bsrValC,
								CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC, int rowBlockDimC, int colBlockDimC, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseDgebsr2gebsr(_handle, dirA, mb, nb, nnzb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA,
				descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer, rowBlockDimC, colBlockDimC, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDgebsr2gebsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in another general
		/// BSR format (that is defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mc+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="bsrColIndC">integer array of nnzc block column indices of the non-zero blocks of matrix C.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C</param>
		/// <param name="colBlockDimC">number of columns within a block of C</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsr_bufferSizeExt.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDimA*colBlockDimA non-zero elements of matrix A.</param>
		/// <param name="bsrValC">array of nnzc*rowBlockDimC*colBlockDimC non-zero elements of matrix C.</param>
		public void Gebsr2gebsr(cusparseDirection dirA, int mb, int nb, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
								CudaDeviceVariable<int> bsrColIndA, int rowBlockDimA, int colBlockDimA, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuFloatComplex> bsrValC,
								CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC, int rowBlockDimC, int colBlockDimC, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseCgebsr2gebsr(_handle, dirA, mb, nb, nnzb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA,
				descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer, rowBlockDimC, colBlockDimC, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCgebsr2gebsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}


		/// <summary>
		/// This function converts a sparse matrix in general BSR format (that is defined by the three
		/// arrays bsrValA, bsrRowPtrA, and bsrColIndA) into a sparse matrix in another general
		/// BSR format (that is defined by arrays bsrValC, bsrRowPtrC, and bsrColIndC).
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="mb">number of block rows of sparse matrix A.</param>
		/// <param name="nb">number of block columns of sparse matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="nnzb">number of nonzero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix A.</param>
		/// <param name="bsrColIndA">integer array of nnzb column indices of the nonzero blocks of matrix A.</param>
		/// <param name="rowBlockDimA">number of rows within a block of A.</param>
		/// <param name="colBlockDimA">number of columns within a block of A.</param>
		/// <param name="descrC">the descriptor of matrix C. The supported matrix 
		/// type is CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases are
		/// CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrRowPtrC">integer array of mc+1 elements that contains the
		/// start of every block row and the end of the last block row plus one of matrix C.</param>
		/// <param name="bsrColIndC">integer array of nnzc block column indices of the non-zero blocks of matrix C.</param>
		/// <param name="rowBlockDimC">number of rows within a block of C</param>
		/// <param name="colBlockDimC">number of columns within a block of C</param>
		/// <param name="buffer">buffer allocated by the user, the size is return by gebsr2gebsr_bufferSizeExt.</param>
		/// <param name="bsrValA">array of nnzb*rowBlockDimA*colBlockDimA non-zero elements of matrix A.</param>
		/// <param name="bsrValC">array of nnzc*rowBlockDimC*colBlockDimC non-zero elements of matrix C.</param>
		public void Gebsr2gebsr(cusparseDirection dirA, int mb, int nb, int nnzb, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA,
								CudaDeviceVariable<int> bsrColIndA, int rowBlockDimA, int colBlockDimA, CudaSparseMatrixDescriptor descrC, CudaDeviceVariable<cuDoubleComplex> bsrValC,
								CudaDeviceVariable<int> bsrRowPtrC, CudaDeviceVariable<int> bsrColIndC, int rowBlockDimC, int colBlockDimC, CudaDeviceVariable<byte> buffer)
		{
			res = CudaSparseNativeMethods.cusparseZgebsr2gebsr(_handle, dirA, mb, nb, nnzb, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, rowBlockDimA, colBlockDimA,
				descrC.Descriptor, bsrValC.DevicePointer, bsrRowPtrC.DevicePointer, bsrColIndC.DevicePointer, rowBlockDimC, colBlockDimC, buffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZgebsr2gebsr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		#endregion




		/// <summary>
		/// This function performs the matrix-vector operation <para/>
		/// y = alpha * op(A) * x + beta * y<para/>
		/// where A is (mb*blockDim) x (nb*blockDim) sparse matrix (that is defined in BSR
		/// storage format by the three arrays bsrVal, bsrRowPtr, and bsrColInd), x and y are
		/// vectors, alpha and beta are scalars. 
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A). Only CUSPARSE_OPERATION_NON_TRANSPOSE is supported.</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="nb">number of block columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtr(mb) - bsrRowPtr(0)) non-zero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtr(m) - bsrRowPtr(0)) column indices of the non-zero blocks of matrix A.
		/// Length of bsrColIndA gives the number nzzb passed to CUSPARSE.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="x">vector of nb*blockDim elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of mb*blockDim element.</param>
		public void Bsrmv(cusparseDirection dirA, cusparseOperation transA, int mb, int nb, float alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaDeviceVariable<float> x, float beta, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseSbsrmv(_handle, dirA, transA, mb, nb, (int)bsrColIndA.Size, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the matrix-vector operation <para/>
		/// y = alpha * op(A) * x + beta * y<para/>
		/// where A is (mb*blockDim) x (nb*blockDim) sparse matrix (that is defined in BSR
		/// storage format by the three arrays bsrVal, bsrRowPtr, and bsrColInd), x and y are
		/// vectors, alpha and beta are scalars. 
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A). Only CUSPARSE_OPERATION_NON_TRANSPOSE is supported.</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="nb">number of block columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtr(mb) - bsrRowPtr(0)) non-zero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtr(m) - bsrRowPtr(0)) column indices of the non-zero blocks of matrix A.
		/// Length of bsrColIndA gives the number nzzb passed to CUSPARSE.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="x">vector of nb*blockDim elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of mb*blockDim element.</param>
		public void Bsrmv(cusparseDirection dirA, cusparseOperation transA, int mb, int nb, double alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaDeviceVariable<double> x, double beta, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDbsrmv(_handle, dirA, transA, mb, nb, (int)bsrColIndA.Size, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the matrix-vector operation <para/>
		/// y = alpha * op(A) * x + beta * y<para/>
		/// where A is (mb*blockDim) x (nb*blockDim) sparse matrix (that is defined in BSR
		/// storage format by the three arrays bsrVal, bsrRowPtr, and bsrColInd), x and y are
		/// vectors, alpha and beta are scalars. 
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A). Only CUSPARSE_OPERATION_NON_TRANSPOSE is supported.</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="nb">number of block columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtr(mb) - bsrRowPtr(0)) non-zero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtr(m) - bsrRowPtr(0)) column indices of the non-zero blocks of matrix A.
		/// Length of bsrColIndA gives the number nzzb passed to CUSPARSE.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="x">vector of nb*blockDim elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of mb*blockDim element.</param>
		public void Bsrmv(cusparseDirection dirA, cusparseOperation transA, int mb, int nb, cuFloatComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaDeviceVariable<cuFloatComplex> x, cuFloatComplex beta, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCbsrmv(_handle, dirA, transA, mb, nb, (int)bsrColIndA.Size, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the matrix-vector operation <para/>
		/// y = alpha * op(A) * x + beta * y<para/>
		/// where A is (mb*blockDim) x (nb*blockDim) sparse matrix (that is defined in BSR
		/// storage format by the three arrays bsrVal, bsrRowPtr, and bsrColInd), x and y are
		/// vectors, alpha and beta are scalars. 
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A). Only CUSPARSE_OPERATION_NON_TRANSPOSE is supported.</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="nb">number of block columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtr(mb) - bsrRowPtr(0)) non-zero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtr(m) - bsrRowPtr(0)) column indices of the non-zero blocks of matrix A.
		/// Length of bsrColIndA gives the number nzzb passed to CUSPARSE.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="x">vector of nb*blockDim elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of mb*blockDim element.</param>
		public void Bsrmv(cusparseDirection dirA, cusparseOperation transA, int mb, int nb, cuDoubleComplex alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaDeviceVariable<cuDoubleComplex> x, cuDoubleComplex beta, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZbsrmv(_handle, dirA, transA, mb, nb, (int)bsrColIndA.Size, ref alpha, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, x.DevicePointer, ref beta, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function performs the matrix-vector operation <para/>
		/// y = alpha * op(A) * x + beta * y<para/>
		/// where A is (mb*blockDim) x (nb*blockDim) sparse matrix (that is defined in BSR
		/// storage format by the three arrays bsrVal, bsrRowPtr, and bsrColInd), x and y are
		/// vectors, alpha and beta are scalars. 
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A). Only CUSPARSE_OPERATION_NON_TRANSPOSE is supported.</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="nb">number of block columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtr(mb) - bsrRowPtr(0)) non-zero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtr(m) - bsrRowPtr(0)) column indices of the non-zero blocks of matrix A.
		/// Length of bsrColIndA gives the number nzzb passed to CUSPARSE.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="x">vector of nb*blockDim elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of mb*blockDim element.</param>
		public void Bsrmv(cusparseDirection dirA, cusparseOperation transA, int mb, int nb, CudaDeviceVariable<float> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaDeviceVariable<float> x, CudaDeviceVariable<float> beta, CudaDeviceVariable<float> y)
		{
			res = CudaSparseNativeMethods.cusparseSbsrmv(_handle, dirA, transA, mb, nb, (int)bsrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseSbsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the matrix-vector operation <para/>
		/// y = alpha * op(A) * x + beta * y<para/>
		/// where A is (mb*blockDim) x (nb*blockDim) sparse matrix (that is defined in BSR
		/// storage format by the three arrays bsrVal, bsrRowPtr, and bsrColInd), x and y are
		/// vectors, alpha and beta are scalars. 
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A). Only CUSPARSE_OPERATION_NON_TRANSPOSE is supported.</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="nb">number of block columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtr(mb) - bsrRowPtr(0)) non-zero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtr(m) - bsrRowPtr(0)) column indices of the non-zero blocks of matrix A.
		/// Length of bsrColIndA gives the number nzzb passed to CUSPARSE.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="x">vector of nb*blockDim elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of mb*blockDim element.</param>
		public void Bsrmv(cusparseDirection dirA, cusparseOperation transA, int mb, int nb, CudaDeviceVariable<double> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaDeviceVariable<double> x, CudaDeviceVariable<double> beta, CudaDeviceVariable<double> y)
		{
			res = CudaSparseNativeMethods.cusparseDbsrmv(_handle, dirA, transA, mb, nb, (int)bsrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDbsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the matrix-vector operation <para/>
		/// y = alpha * op(A) * x + beta * y<para/>
		/// where A is (mb*blockDim) x (nb*blockDim) sparse matrix (that is defined in BSR
		/// storage format by the three arrays bsrVal, bsrRowPtr, and bsrColInd), x and y are
		/// vectors, alpha and beta are scalars. 
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A). Only CUSPARSE_OPERATION_NON_TRANSPOSE is supported.</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="nb">number of block columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtr(mb) - bsrRowPtr(0)) non-zero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtr(m) - bsrRowPtr(0)) column indices of the non-zero blocks of matrix A.
		/// Length of bsrColIndA gives the number nzzb passed to CUSPARSE.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="x">vector of nb*blockDim elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of mb*blockDim element.</param>
		public void Bsrmv(cusparseDirection dirA, cusparseOperation transA, int mb, int nb, CudaDeviceVariable<cuFloatComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaDeviceVariable<cuFloatComplex> x, CudaDeviceVariable<cuFloatComplex> beta, CudaDeviceVariable<cuFloatComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseCbsrmv(_handle, dirA, transA, mb, nb, (int)bsrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCbsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}
		/// <summary>
		/// This function performs the matrix-vector operation <para/>
		/// y = alpha * op(A) * x + beta * y<para/>
		/// where A is (mb*blockDim) x (nb*blockDim) sparse matrix (that is defined in BSR
		/// storage format by the three arrays bsrVal, bsrRowPtr, and bsrColInd), x and y are
		/// vectors, alpha and beta are scalars. 
		/// </summary>
		/// <param name="dirA">storage format of blocks, either CUSPARSE_DIRECTION_ROW or CUSPARSE_DIRECTION_COLUMN.</param>
		/// <param name="transA">the operation op(A). Only CUSPARSE_OPERATION_NON_TRANSPOSE is supported.</param>
		/// <param name="mb">number of block rows of matrix A.</param>
		/// <param name="nb">number of block columns of matrix A.</param>
		/// <param name="alpha">scalar used for multiplication.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL. Also, the supported index bases
		/// are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="bsrValA">array of nnzb (= bsrRowPtr(mb) - bsrRowPtr(0)) non-zero blocks of matrix A.</param>
		/// <param name="bsrRowPtrA">integer array of mb+1 elements that contains the start of every block
		/// row and the end of the last block row plus one.</param>
		/// <param name="bsrColIndA">integer array of nnzb (= bsrRowPtr(m) - bsrRowPtr(0)) column indices of the non-zero blocks of matrix A.
		/// Length of bsrColIndA gives the number nzzb passed to CUSPARSE.</param>
		/// <param name="blockDim">block dimension of sparse matrix A, larger than zero.</param>
		/// <param name="x">vector of nb*blockDim elements.</param>
		/// <param name="beta">scalar used for multiplication. If beta is zero, y does not have to be a valid input.</param>
		/// <param name="y">vector of mb*blockDim element.</param>
		public void Bsrmv(cusparseDirection dirA, cusparseOperation transA, int mb, int nb, CudaDeviceVariable<cuDoubleComplex> alpha, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> bsrValA, CudaDeviceVariable<int> bsrRowPtrA, CudaDeviceVariable<int> bsrColIndA, int blockDim, CudaDeviceVariable<cuDoubleComplex> x, CudaDeviceVariable<cuDoubleComplex> beta, CudaDeviceVariable<cuDoubleComplex> y)
		{
			res = CudaSparseNativeMethods.cusparseZbsrmv(_handle, dirA, transA, mb, nb, (int)bsrColIndA.Size, alpha.DevicePointer, descrA.Descriptor, bsrValA.DevicePointer, bsrRowPtrA.DevicePointer, bsrColIndA.DevicePointer, blockDim, x.DevicePointer, beta.DevicePointer, y.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZbsrmv", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}














		/* --- Sparse Matrix Sorting --- */

		/* Description: Create a identity sequence p=[0,1,...,n-1]. */

		/// <summary>
		/// This function creates an identity map. The output parameter p represents such map by p = 0:1:(n-1).<para/>
		/// This function is typically used with coosort, csrsort, cscsort, csr2csc_indexOnly.
		/// </summary>
		/// <param name="n">size of the map.</param>
		/// <param name="p">integer array of dimensions n.</param>
		public void CreateIdentityPermutation(int n, CudaDeviceVariable<int> p)
		{
			res = CudaSparseNativeMethods.cusparseCreateIdentityPermutation(_handle, n, p.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCreateIdentityPermutation", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/* Description: Sort sparse matrix stored in COO format */

		/// <summary>
		/// This function sorts COO format. The stable sorting is in-place. Also the user can sort by row or sort by column.<para/>
		/// A is an m x n sparse matrix that is defined in COO storage format by the three arrays cooVals, cooRows, and cooCols.<para/>
		/// The matrix must be base 0.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="cooRowsA">integer array of nnz unsorted row indices of A.</param>
		/// <param name="cooColsA">integer array of nnz unsorted column indices of A.</param>
		/// <returns>number of bytes of the buffer.</returns>
		public SizeT CoosortBufferSize(int m, int n, int nnz, CudaDeviceVariable<int> cooRowsA, CudaDeviceVariable<int> cooColsA) 
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseXcoosort_bufferSizeExt(_handle, m, n, nnz, cooRowsA.DevicePointer, cooColsA.DevicePointer, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcoosort_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function sorts COO format. The stable sorting is in-place. Also the user can sort by row or sort by column.<para/>
		/// A is an m x n sparse matrix that is defined in COO storage format by the three arrays cooVals, cooRows, and cooCols.<para/>
		/// The matrix must be base 0.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="cooRowsA">integer array of nnz unsorted row indices of A.</param>
		/// <param name="cooColsA">integer array of nnz unsorted column indices of A.</param>
		/// <param name="P">integer array of nnz sorted map indices.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by CoosortBufferSize().</param>
		public void CoosortByRow(int m, int n, int nnz, CudaDeviceVariable<int> cooRowsA, CudaDeviceVariable<int> cooColsA, CudaDeviceVariable<int> P, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseXcoosortByRow(_handle, m, n, nnz, cooRowsA.DevicePointer, cooColsA.DevicePointer, P.DevicePointer, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcoosortByRow", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function sorts COO format. The stable sorting is in-place. Also the user can sort by row or sort by column.<para/>
		/// A is an m x n sparse matrix that is defined in COO storage format by the three arrays cooVals, cooRows, and cooCols.<para/>
		/// The matrix must be base 0.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="cooRowsA">integer array of nnz unsorted row indices of A.</param>
		/// <param name="cooColsA">integer array of nnz unsorted column indices of A.</param>
		/// <param name="P">integer array of nnz sorted map indices.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by CoosortBufferSize().</param>
		public void CoosortByColumn(int m, int n, int nnz, CudaDeviceVariable<int> cooRowsA, CudaDeviceVariable<int> cooColsA, CudaDeviceVariable<int> P, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseXcoosortByColumn(_handle, m, n, nnz, cooRowsA.DevicePointer, cooColsA.DevicePointer, P.DevicePointer, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcoosortByColumn", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		} 

		/* Description: Sort sparse matrix stored in CSR format */

		/// <summary>
		/// This function sorts CSR format. The stable sorting is in-place.<para/>
		/// The matrix type is regarded as CUSPARSE_MATRIX_TYPE_GENERAL implicitly. In other
		/// words, any symmetric property is ignored.<para/>
		/// This function csrsort() requires buffer size returned by csrsort_bufferSizeExt().<para/>
		/// The address of pBuffer must be multiple of 128 bytes. If not,
		/// CUSPARSE_STATUS_INVALID_VALUE is returned.<para/>
		/// The parameter P is both input and output. If the user wants to compute sorted csrVal,
		/// P must be set as 0:1:(nnz-1) before csrsort(), and after csrsort(), new sorted value
		/// array satisfies csrVal_sorted = csrVal(P).
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz unsorted column indices of A.</param>
		/// <returns>number of bytes of the buffer.</returns>
		public SizeT CsrsortBufferSize(int m, int n, int nnz, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseXcsrsort_bufferSizeExt(_handle, m, n, nnz, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrsort_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function sorts CSR format. The stable sorting is in-place.<para/>
		/// The matrix type is regarded as CUSPARSE_MATRIX_TYPE_GENERAL implicitly. In other
		/// words, any symmetric property is ignored.<para/>
		/// This function csrsort() requires buffer size returned by csrsort_bufferSizeExt().<para/>
		/// The address of pBuffer must be multiple of 128 bytes. If not,
		/// CUSPARSE_STATUS_INVALID_VALUE is returned.<para/>
		/// The parameter P is both input and output. If the user wants to compute sorted csrVal,
		/// P must be set as 0:1:(nnz-1) before csrsort(), and after csrsort(), new sorted value
		/// array satisfies csrVal_sorted = csrVal(P).
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="csrRowPtrA">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColIndA">integer array of nnz unsorted column indices of A.</param>
		/// <param name="P">integer array of nnz sorted map indices.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by CsrsortBufferSize().</param>
		public void Csrsort(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> csrRowPtrA, CudaDeviceVariable<int> csrColIndA, CudaDeviceVariable<int> P, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseXcsrsort(_handle, m, n, nnz, descrA.Descriptor, csrRowPtrA.DevicePointer, csrColIndA.DevicePointer, P.DevicePointer, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcsrsort", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/* Description: Sort sparse matrix stored in CSC format */

		/// <summary>
		/// This function sorts CSC format. The stable sorting is in-place.<para/>
		/// The matrix type is regarded as CUSPARSE_MATRIX_TYPE_GENERAL implicitly. In other
		/// words, any symmetric property is ignored. <para/>
		/// This function cscsort() requires buffer size returned by cscsort_bufferSizeExt().
		/// The address of pBuffer must be multiple of 128 bytes. If not,
		/// CUSPARSE_STATUS_INVALID_VALUE is returned.<para/>
		/// The parameter P is both input and output. If the user wants to compute sorted cscVal,
		/// P must be set as 0:1:(nnz-1) before cscsort(), and after cscsort(), new sorted value
		/// array satisfies cscVal_sorted = cscVal(P).
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="cscColPtrA">integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="cscRowIndA">integer array of nnz unsorted row indices of A.</param>
		/// <returns>number of bytes of the buffer.</returns>
		public SizeT CscsortBufferSize(int m, int n, int nnz, CudaDeviceVariable<int> cscColPtrA, CudaDeviceVariable<int> cscRowIndA)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseXcscsort_bufferSizeExt(_handle, m, n, nnz, cscColPtrA.DevicePointer, cscRowIndA.DevicePointer, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcscsort_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function sorts CSC format. The stable sorting is in-place.<para/>
		/// The matrix type is regarded as CUSPARSE_MATRIX_TYPE_GENERAL implicitly. In other
		/// words, any symmetric property is ignored. <para/>
		/// This function cscsort() requires buffer size returned by cscsort_bufferSizeExt().
		/// The address of pBuffer must be multiple of 128 bytes. If not,
		/// CUSPARSE_STATUS_INVALID_VALUE is returned.<para/>
		/// The parameter P is both input and output. If the user wants to compute sorted cscVal,
		/// P must be set as 0:1:(nnz-1) before cscsort(), and after cscsort(), new sorted value
		/// array satisfies cscVal_sorted = cscVal(P).
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A.</param>
		/// <param name="cscColPtrA">integer array of n+1 elements that contains the start of every column and the end of the last column plus one.</param>
		/// <param name="cscRowIndA">integer array of nnz unsorted row indices of A.</param>
		/// <param name="P">integer array of nnz sorted map indices.</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by CscsortBufferSize().</param>
		public void Cscsort(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<int> cscColPtrA, CudaDeviceVariable<int> cscRowIndA, CudaDeviceVariable<int> P, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseXcscsort(_handle, m, n, nnz, descrA.Descriptor, cscColPtrA.DevicePointer, cscRowIndA.DevicePointer, P.DevicePointer, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseXcscsort", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/* Description: Wrapper that sorts sparse matrix stored in CSR format 
		   (without exposing the permutation). */

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <returns>number of bytes of the buffer.</returns>
		public SizeT Csru2csrBufferSize(int m, int n, int nnz, CudaDeviceVariable<float> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseScsru2csr_bufferSizeExt(_handle, m, n, nnz, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsru2csr_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <returns>number of bytes of the buffer.</returns>
		public SizeT Csru2csrBufferSize(int m, int n, int nnz, CudaDeviceVariable<double> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseDcsru2csr_bufferSizeExt(_handle, m, n, nnz, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsru2csr_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <returns>number of bytes of the buffer.</returns>
		public SizeT Csru2csrBufferSize(int m, int n, int nnz, CudaDeviceVariable<cuFloatComplex> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseCcsru2csr_bufferSizeExt(_handle, m, n, nnz, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsru2csr_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <returns>number of bytes of the buffer.</returns>
		public SizeT Csru2csrBufferSize(int m, int n, int nnz, CudaDeviceVariable<cuDoubleComplex> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info)
		{
			SizeT size = 0;
			res = CudaSparseNativeMethods.cusparseZcsru2csr_bufferSizeExt(_handle, m, n, nnz, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, ref size);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsru2csr_bufferSizeExt", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
			return size;
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL, Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by Csru2csrBufferSize().</param>
		public void Csru2csr(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseScsru2csr(_handle, m, n, nnz, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsru2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL, Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by Csru2csrBufferSize().</param>
		public void Csru2csr(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsru2csr(_handle, m, n, nnz, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsru2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL, Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by Csru2csrBufferSize().</param>
		public void Csru2csr(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsru2csr(_handle, m, n, nnz, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsru2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL, Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by Csru2csrBufferSize().</param>
		public void Csru2csr(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsru2csr(_handle, m, n, nnz, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsru2csr", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/* Description: Wrapper that un-sorts sparse matrix stored in CSR format 
		   (without exposing the permutation). */

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL, Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by Csru2csrBufferSize().</param>
		public void Csr2csru(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<float> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseScsr2csru(_handle, m, n, nnz, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseScsr2csru", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL, Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by Csru2csrBufferSize().</param>
		public void Csr2csru(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<double> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseDcsr2csru(_handle, m, n, nnz, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseDcsr2csru", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL, Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by Csru2csrBufferSize().</param>
		public void Csr2csru(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuFloatComplex> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseCcsr2csru(_handle, m, n, nnz, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseCcsr2csru", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}

		/// <summary>
		/// This function transfers unsorted CSR format to CSR format, and vice versa. The operation is in-place.
		/// </summary>
		/// <param name="m">number of rows of matrix A.</param>
		/// <param name="n">number of columns of matrix A.</param>
		/// <param name="nnz">number of nonzero elements of matrix A.</param>
		/// <param name="descrA">the descriptor of matrix A. The supported matrix type is
		/// CUSPARSE_MATRIX_TYPE_GENERAL, Also, the supported index bases are CUSPARSE_INDEX_BASE_ZERO and CUSPARSE_INDEX_BASE_ONE.</param>
		/// <param name="csrVal">array of nnz unsorted nonzero elements of matrix A.</param>
		/// <param name="csrRowPtr">integer array of m+1 elements that contains the start of every row and the end of the last row plus one.</param>
		/// <param name="csrColInd">integer array of nnz unsorted column indices of A.</param>
		/// <param name="info">opaque structure initialized using cusparseCreateCsru2csrInfo().</param>
		/// <param name="pBuffer">buffer allocated by the user; the size is returned by Csru2csrBufferSize().</param>
		public void Csr2csru(int m, int n, int nnz, CudaSparseMatrixDescriptor descrA, CudaDeviceVariable<cuDoubleComplex> csrVal, CudaDeviceVariable<int> csrRowPtr, CudaDeviceVariable<int> csrColInd, CudaSparseCsru2csrInfo info, CudaDeviceVariable<byte> pBuffer)
		{
			res = CudaSparseNativeMethods.cusparseZcsr2csru(_handle, m, n, nnz, descrA.Descriptor, csrVal.DevicePointer, csrRowPtr.DevicePointer, csrColInd.DevicePointer, info.Csru2csrInfo, pBuffer.DevicePointer);
			Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cusparseZcsr2csru", res));
			if (res != cusparseStatus.Success)
				throw new CudaSparseException(res);
		}



		#endregion

		/// <summary>
		/// Returns the wrapped cusparseContext handle
		/// </summary>
		public cusparseContext Handle
		{
			get { return _handle; }
		}
	}
}