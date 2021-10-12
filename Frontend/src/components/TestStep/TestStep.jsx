import * as yup from 'yup';

import { Box, Button, TextField, Typography } from '@mui/material';
import { Controller, useForm } from 'react-hook-form';
import React, { useEffect, useState } from 'react';

import AddIcon from '@mui/icons-material/Add';
import CloseIcon from '@mui/icons-material/Close';
import CreateIcon from '@mui/icons-material/Create';

import PropTypes from 'prop-types';
import WarningIcon from '@mui/icons-material/Warning';
import { yupResolver } from '@hookform/resolvers/yup';
import EditableTable from '../EditableTable/EditableTable';

const formFieldsTable = {
  rowsNumber: 'rowsNumber',
  columnsNumber: 'columnsNumber'
};

const defaultValuesTable = {
  [formFieldsTable.rowsNumber]: '',
  [formFieldsTable.columnsNumber]: ''
};

const schemaTable = yup.object().shape({
  [formFieldsTable.rowsNumber]: yup.number().required().min(1).max(10),
  [formFieldsTable.columnsNumber]: yup.number().required().min(1).max(10)
});

const formFieldsStepName = {
  stepName: 'stepName'
};

const schemaStepName = yup.object().shape({
  [formFieldsStepName.stepName]: yup.string().required()
});

// TODO: connect with outer control form
// TODO: usable API
// TODO: styles
// part of test creation form
const TestStep = ({ testPlanName, testName, testProcedureName, testStepName, isEditable }) => {
  const {
    control: innerControlTable,
    handleSubmit: handleSubmitTable,
    reset: resetTable,
    formState: { errors: errorsTable }
  } = useForm({
    defaultValuesTable,
    resolver: yupResolver(schemaTable)
  });

  const {
    control: innerControlStepName,
    handleSubmit: handleSubmitStepName,
    reset: resetStepName,
    formState: { errors: errorsStepName }
  } = useForm({
    resolver: yupResolver(schemaStepName)
  });

  const [isOpened, setIsOpened] = useState(false);
  const [stepName, setStepName] = useState(testStepName);
  const [tablesCount, setTablesCount] = useState(0);
  const [editableTables, setEditableTables] = useState([
    {
      tableName: 'table-demo',
      rowsNumber: 5,
      columnsNumber: 8
    }
  ]);

  const [isEditing, setIsEditing] = useState(false);
  const [isAddingTable, setIsAddingTable] = useState(false);
  const [isEditingStepName, setIsEditingStepName] = useState(false);

  useEffect(() => { }, []);

  const addTable = ({ rowsNumber, columnsNumber }) => {
    setTablesCount((state) => state + 1);
    setEditableTables((state) => [
      ...state,
      {
        tableName: `table-${tablesCount}`,
        rowsNumber,
        columnsNumber
      }
    ]);
    setIsAddingTable(false);
    resetTable(defaultValuesTable, {
      keepIsValid: true
    });
  };

  const changeStepName = ({ stepName }) => {
    setStepName(stepName);
    setIsEditingStepName(false);
  };

  const deleteTable = (tableName) => {
    setEditableTables((state) => [...state.filter((table) => table.tableName !== tableName)]);
  };

  return (
    <Box
      sx={{
        width: '100%',
        position: 'relative'
      }}
    >
      <Button
        onClick={
          !isEditing
            ? () => setIsOpened((state) => !state)
            : () => alert('Cannot close the view in an editing mode!')
        }
        variant="contained"
        sx={{
          width: '100%',
          bgcolor: '#0077c2',
          color: 'white',
          textTransform: 'capitalize'
        }}
      >
        {stepName}
      </Button>
      {isOpened && (
        <Box>
          {isEditable && !isEditing && (
            <CreateIcon
              sx={{
                position: 'absolute',
                top: '6vh',
                right: '2vw',
                border: '1px solid black',
                borderRadius: '50%',
                padding: '2px',
                '&:hover': {
                  cursor: 'pointer'
                }
              }}
              onClick={() => setIsEditing(true)}
            />
          )}
          <Box>
            {isEditing && (!isEditingStepName ? (
              <Button onClick={() => setIsEditingStepName(true)} variant="outlined">Edit Step Name</Button>
            ) : (
              <Box component="form" onSubmit={handleSubmitStepName(changeStepName)}>
                <Controller
                  shouldUnregister
                  name={formFieldsStepName.stepName}
                  control={innerControlStepName}
                  render={({ field }) => (
                    <TextField
                      id={formFieldsStepName.stepName}
                      label="Step Name"
                      type="text"
                      defaultValue={testStepName}
                      error={!!errorsStepName.stepName}
                      helperText={
                        !!errorsStepName.stepName &&
                        'Step Name field is required!'
                      }
                      {...field}
                      sx={{
                        marginTop: '0.625rem',
                        width: '10rem'
                      }}
                    />
                  )}
                />
                <Button
                  type="submit"
                  variant="outlined"
                  sx={{
                    height: '3.125rem',
                    width: '12rem',
                    margin: '0.625rem 0.625rem 1.25rem 0.625rem'
                  }}
                // startIcon={<AddIcon />}
                >
                  Change Step Name
                </Button>
                <Button
                  variant="outlined"
                  sx={{
                    height: '3.125rem',
                    width: '7rem',
                    marginTop: '0.625rem',
                    marginBottom: '1.25rem'
                  }}
                  onClick={() => {
                    setIsEditingStepName(false);
                    resetStepName({ 'stepName': testStepName });
                  }}
                  startIcon={<CloseIcon />}
                >
                  Close
                </Button>
              </Box>
            ))}
          </Box>
          <Box>
            <Typography
              variant="h5"
              sx={{
                marginTop: '0.625rem'
              }}
            >
              Test Data:
            </Typography>
            {editableTables.length > 0 ? (
              <Box>
                {editableTables.map(({ tableName, rowsNumber, columnsNumber }) => (
                  <EditableTable
                    key={`${tableName}`}
                    name={`${testPlanName}-${testName}-${testProcedureName}-${testStepName}-${tableName}`}
                    rowsNumber={rowsNumber}
                    columnsNumber={columnsNumber}
                    disabled={!isEditing}
                    deleteTable={() => deleteTable(tableName)}
                  // TODO: Add control props from outer form !!!
                  />
                ))}
              </Box>
            ) : (
              <Button
                sx={{
                  '&.Mui-disabled': {
                    color: 'black'
                  }
                }}
                variant="body1"
                startIcon={<WarningIcon />}
                disabled
              >
                No Data
              </Button>
            )}
            {isEditing &&
              (!isAddingTable ? (
                <Button
                  variant="outlined"
                  sx={{ marginTop: '0.625rem' }}
                  onClick={() => setIsAddingTable(true)}
                  startIcon={<AddIcon />}
                >
                  Add table
                </Button>
              ) : (
                <Box component="form" onSubmit={handleSubmitTable(addTable)}>
                  <Controller
                    shouldUnregister
                    name={formFieldsTable.rowsNumber}
                    control={innerControlTable}
                    render={({ field }) => (
                      <TextField
                        id={formFieldsTable.rowsNumber}
                        label="Rows Number"
                        type="text"
                        error={!!errorsTable.rowsNumber}
                        helperText={
                          !!errorsTable.rowsNumber &&
                          'Rows number is required and must belong to [1-10]!'
                        }
                        {...field}
                        sx={{
                          marginTop: '0.625rem',
                          width: '10rem'
                        }}
                      />
                    )}
                  />
                  <Controller
                    shouldUnregister
                    name={formFieldsTable.columnsNumber}
                    control={innerControlTable}
                    render={({ field }) => (
                      <TextField
                        id={formFieldsTable.columnsNumber}
                        label="Columns Number"
                        type="text"
                        error={!!errorsTable.columnsNumber}
                        helperText={
                          !!errorsTable.columnsNumber &&
                          'Columns number is required and must belong to [1-10]!'
                        }
                        {...field}
                        sx={{
                          marginTop: '0.625rem',
                          width: '10rem'
                        }}
                      />
                    )}
                  />
                  <Button
                    type="submit"
                    variant="outlined"
                    sx={{
                      height: '3.125rem',
                      width: '7rem',
                      margin: '0.625rem 0.625rem 1.25rem 0.625rem'
                    }}
                    startIcon={<AddIcon />}
                  >
                    Add
                  </Button>
                  <Button
                    variant="outlined"
                    sx={{
                      height: '3.125rem',
                      width: '7rem',
                      marginTop: '0.625rem',
                      marginBottom: '1.25rem'
                    }}
                    onClick={() => {
                      setIsAddingTable(false);
                      resetTable(defaultValuesTable);
                    }}
                    startIcon={<CloseIcon />}
                  >
                    Close
                  </Button>
                </Box>
              ))}
          </Box>
          <Box>
            <Typography sx={{ marginTop: '1.25rem' }} variant="h5">
              Control Point:
            </Typography>
            <Controller
              name={`${testPlanName}-${testName}-${testProcedureName}-${testStepName}-result`}
              control={innerControlTable} // TODO: Change into outer controller
              render={({ field }) => (
                <TextField
                  id={`${testPlanName}-${testName}-${testProcedureName}-${testStepName}-result`}
                  label="Control Point"
                  type="text"
                  disabled={!isEditing}
                  error=""
                  helperText=""
                  multiline
                  rows={3}
                  {...field}
                  sx={{
                    marginTop: '0.625rem',
                    width: '100%'
                  }}
                />
              )}
            />
          </Box>
          {isEditing && (
            <Button
              variant="outlined"
              sx={{ marginTop: '0.625rem' }}
              onClick={() => {
                setIsEditing(false);
                setIsAddingTable(false);
              }}
            >
              Save
            </Button>
          )}
        </Box>
      )
      }
    </Box >
  );
};

TestStep.propTypes = {
  testPlanName: PropTypes.string.isRequired,
  testName: PropTypes.string.isRequired,
  testProcedureName: PropTypes.string.isRequired,
  testStepName: PropTypes.string.isRequired,
  isEditable: PropTypes.bool.isRequired
  // control: PropTypes.object.isRequired
};

export default TestStep;